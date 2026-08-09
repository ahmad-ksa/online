using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// مدير اختيار الشخصيات - يتعامل مع اختيار وتخصيص الشخصيات
/// </summary>
public class CharacterSelectionManager : MonoBehaviour
{
    private static CharacterSelectionManager instance;

    private List<CharacterTemplate> availableCharacters = new List<CharacterTemplate>();
    private CharacterTemplate selectedCharacter = null;
    private CharacterCustomization currentCustomization = new CharacterCustomization();

    // Events
    public static event Action<CharacterTemplate> OnCharacterSelected;
    public static event Action<CharacterCustomization> OnCustomizationChanged;
    public static event Action<CharacterTemplate> OnCharacterUnlocked;

    // Storage Keys
    private const string SELECTED_CHARACTER_KEY = "SelectedCharacter";
    private const string CHARACTER_CUSTOMIZATION_KEY = "CharacterCustomization";

    private bool isInitialized = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// تهيئة مدير اختيار الشخصيات
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
            return;

        LoadAvailableCharacters();
        LoadSelectedCharacterLocal();
        LoadCustomizationLocal();

        isInitialized = true;

        Debug.Log("CharacterSelectionManager Initialized");
    }

    /// <summary>
    /// تحميل الشخصيات المتاحة
    /// </summary>
    private void LoadAvailableCharacters()
    {
        try
        {
            // محاكاة تحميل الشخصيات من Config
            var config = ConfigurationManager.Instance.GameConfig;

            // إنشاء شخصيات افتراضية
            availableCharacters = new List<CharacterTemplate>
            {
                new CharacterTemplate
                {
                    CharacterId = "knight",
                    Name = "Knight",
                    Description = "A strong warrior with high HP",
                    Health = 120,
                    Speed = 4,
                    Attack = 12,
                    Defense = 8,
                    IsUnlocked = true,
                    UnlockCost = 0
                },
                new CharacterTemplate
                {
                    CharacterId = "archer",
                    Name = "Archer",
                    Description = "Fast and precise ranged attacker",
                    Health = 80,
                    Speed = 6,
                    Attack = 10,
                    Defense = 4,
                    IsUnlocked = false,
                    UnlockCost = 500
                },
                new CharacterTemplate
                {
                    CharacterId = "mage",
                    Name = "Mage",
                    Description = "Powerful magic user with AoE abilities",
                    Health = 70,
                    Speed = 5,
                    Attack = 14,
                    Defense = 3,
                    IsUnlocked = false,
                    UnlockCost = 750
                },
                new CharacterTemplate
                {
                    CharacterId = "assassin",
                    Name = "Assassin",
                    Description = "Stealth and high burst damage",
                    Health = 75,
                    Speed = 7,
                    Attack = 13,
                    Defense = 2,
                    IsUnlocked = false,
                    UnlockCost = 1000
                }
            };

            // تحديث الشخصيات المفتوحة من PlayerStats
            var unlockedChars = PlayerManager.Instance.CurrentStats.UnlockedCharacters;
            foreach (var character in availableCharacters)
            {
                character.IsUnlocked = unlockedChars.Contains(character.CharacterId);
            }

            Debug.Log($"Loaded {availableCharacters.Count} characters");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load characters: {ex.Message}");
        }
    }

    /// <summary>
    /// الحصول على جميع الشخصيات المتاحة
    /// </summary>
    public List<CharacterTemplate> GetAvailableCharacters()
    {
        return new List<CharacterTemplate>(availableCharacters);
    }

    /// <summary>
    /// الحصول على الشخصيات المفتوحة فقط
    /// </summary>
    public List<CharacterTemplate> GetUnlockedCharacters()
    {
        return availableCharacters.Where(c => c.IsUnlocked).ToList();
    }

    /// <summary>
    /// اختيار شخصية
    /// </summary>
    public async Task<bool> SelectCharacter(string characterId)
    {
        var character = availableCharacters.FirstOrDefault(c => c.CharacterId == characterId);

        if (character == null)
        {
            Debug.LogWarning($"Character not found: {characterId}");
            return false;
        }

        if (!character.IsUnlocked)
        {
            Debug.LogWarning($"Character not unlocked: {characterId}");
            return false;
        }

        try
        {
            Debug.Log($"Selecting character: {character.Name}");

            await Task.Delay(300);

            selectedCharacter = character;
            PlayerManager.Instance.SetSelectedCharacter(characterId);
            SaveSelectedCharacterLocal();

            OnCharacterSelected?.Invoke(character);

            Debug.Log($"Character selected: {character.Name}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to select character: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تعديل تخصيص الشخصية
    /// </summary>
    public async Task<bool> CustomizeCharacter(CharacterCustomization customization)
    {
        if (selectedCharacter == null)
        {
            Debug.LogWarning("No character selected!");
            return false;
        }

        try
        {
            Debug.Log($"Customizing character: {selectedCharacter.Name}");

            await Task.Delay(200);

            currentCustomization = customization;
            SaveCustomizationLocal();

            OnCustomizationChanged?.Invoke(customization);

            Debug.Log("Character customized");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to customize character: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// فتح شخصية
    /// </summary>
    public async Task<bool> UnlockCharacter(string characterId)
    {
        var character = availableCharacters.FirstOrDefault(c => c.CharacterId == characterId);

        if (character == null)
        {
            Debug.LogWarning($"Character not found: {characterId}");
            return false;
        }

        if (character.IsUnlocked)
        {
            Debug.LogWarning($"Character already unlocked: {characterId}");
            return false;
        }

        try
        {
            Debug.Log($"Unlocking character: {character.Name} (Cost: {character.UnlockCost})");

            // التحقق من العملات
            if (!PlayerManager.Instance.SpendCurrency(character.UnlockCost, CurrencyType.Coins))
            {
                Debug.LogWarning("Not enough coins!");
                return false;
            }

            await Task.Delay(500);

            character.IsUnlocked = true;
            PlayerManager.Instance.UnlockCharacter(characterId);
            SaveAvailableCharactersLocal();

            OnCharacterUnlocked?.Invoke(character);

            Debug.Log($"Character unlocked: {character.Name}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to unlock character: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// شراء Skin للشخصية
    /// </summary>
    public async Task<bool> BuySkin(string characterId, string skinId, int cost)
    {
        try
        {
            Debug.Log($"Buying skin: {skinId} for {characterId}");

            if (!PlayerManager.Instance.SpendCurrency(cost, CurrencyType.Gems))
            {
                Debug.LogWarning("Not enough gems!");
                return false;
            }

            await Task.Delay(300);

            // إضافة Skin للتخصيص
            if (!currentCustomization.OwnedSkins.Contains(skinId))
            {
                currentCustomization.OwnedSkins.Add(skinId);
                SaveCustomizationLocal();
            }

            Debug.Log($"Skin purchased: {skinId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to buy skin: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تطبيق Skin
    /// </summary>
    public async Task<bool> ApplySkin(string skinId)
    {
        if (!currentCustomization.OwnedSkins.Contains(skinId) && skinId != "default")
        {
            Debug.LogWarning($"Skin not owned: {skinId}");
            return false;
        }

        try
        {
            Debug.Log($"Applying skin: {skinId}");

            await Task.Delay(100);

            currentCustomization.CurrentSkin = skinId;
            SaveCustomizationLocal();

            OnCustomizationChanged?.Invoke(currentCustomization);

            Debug.Log($"Skin applied: {skinId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to apply skin: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تغيير لون الشخصية
    /// </summary>
    public async Task<bool> ChangeColor(Color newColor)
    {
        try
        {
            Debug.Log("Changing character color...");

            await Task.Delay(100);

            currentCustomization.Color = newColor;
            SaveCustomizationLocal();

            OnCustomizationChanged?.Invoke(currentCustomization);

            Debug.Log("Color changed");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to change color: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// الحصول على إحصائيات الشخصية المحددة
    /// </summary>
    public CharacterStats GetSelectedCharacterStats()
    {
        if (selectedCharacter == null)
            return null;

        return new CharacterStats
        {
            CharacterId = selectedCharacter.CharacterId,
            CharacterName = selectedCharacter.Name,
            Health = selectedCharacter.Health,
            Speed = selectedCharacter.Speed,
            Attack = selectedCharacter.Attack,
            Defense = selectedCharacter.Defense,
            CurrentSkin = currentCustomization.CurrentSkin,
            Color = currentCustomization.Color
        };
    }

    /// <summary>
    /// حفظ الشخصية المختارة
    /// </summary>
    private void SaveSelectedCharacterLocal()
    {
        try
        {
            if (selectedCharacter != null)
            {
                string json = JsonUtility.ToJson(selectedCharacter);
                PlayerPrefs.SetString(SELECTED_CHARACTER_KEY, json);
                PlayerPrefs.Save();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save selected character: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل الشخصية المختارة
    /// </summary>
    private void LoadSelectedCharacterLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(SELECTED_CHARACTER_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                selectedCharacter = JsonUtility.FromJson<CharacterTemplate>(json);
                Debug.Log($"Selected character loaded: {selectedCharacter.Name}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load selected character: {ex.Message}");
        }
    }

    /// <summary>
    /// حفظ التخصيص
    /// </summary>
    private void SaveCustomizationLocal()
    {
        try
        {
            string json = JsonUtility.ToJson(currentCustomization);
            PlayerPrefs.SetString(CHARACTER_CUSTOMIZATION_KEY, json);
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save customization: {ex.Message}");
        }
    }

    /// <summary>
    /// تحميل التخصيص
    /// </summary>
    private void LoadCustomizationLocal()
    {
        try
        {
            string json = PlayerPrefs.GetString(CHARACTER_CUSTOMIZATION_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                currentCustomization = JsonUtility.FromJson<CharacterCustomization>(json);
                Debug.Log("Character customization loaded");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load customization: {ex.Message}");
        }
    }

    /// <summary>
    /// حفظ الشخصيات المتاحة
    /// </summary>
    private void SaveAvailableCharactersLocal()
    {
        try
        {
            // يمكن حفظ حالة الشخصيات المفتوحة
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save characters: {ex.Message}");
        }
    }

    // Getters
    public static CharacterSelectionManager Instance => instance;
    public CharacterTemplate SelectedCharacter => selectedCharacter;
    public CharacterCustomization CurrentCustomization => currentCustomization;
}

// ==================== Data Classes ====================

[System.Serializable]
public class CharacterTemplate
{
    public string CharacterId;
    public string Name;
    public string Description;
    public float Health;
    public float Speed;
    public float Attack;
    public float Defense;
    public bool IsUnlocked;
    public int UnlockCost;
}

[System.Serializable]
public class CharacterCustomization
{
    public string CurrentSkin = "default";
    [SerializeField]
    private List<string> ownedSkins = new List<string> { "default" };
    public Color Color = Color.white;
    public string CustomName = "";

    public List<string> OwnedSkins => ownedSkins;
}

[System.Serializable]
public class CharacterStats
{
    public string CharacterId;
    public string CharacterName;
    public float Health;
    public float Speed;
    public float Attack;
    public float Defense;
    public string CurrentSkin;
    public Color Color;
}
