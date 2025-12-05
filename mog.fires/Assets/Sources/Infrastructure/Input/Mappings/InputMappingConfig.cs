using System;
using System.Collections.Generic;
using Sources.Infrastructure.Input.Actions;
using UnityEngine;

namespace Sources.Infrastructure.Input.Mappings
{
    [Serializable]
    public class KeyboardMapping
    {
        public KeyCode Key;
        public InputActionType Action;
    }
    
    [Serializable]
    public class SerialButtonMapping
    {
        public int ButtonId;
        public InputActionType Action;
    }
    
    [CreateAssetMenu(fileName = "InputMappingConfig", menuName = "Config/Input Mapping")]
    public class InputMappingConfig : ScriptableObject
    {
        [Header("Keyboard Mappings")]
        public List<KeyboardMapping> KeyboardMappings = new()
        {
            new KeyboardMapping { Key = KeyCode.L,         Action = InputActionType.ChangeLanguage },
            new KeyboardMapping { Key = KeyCode.R,         Action = InputActionType.NavigateHome },
            new KeyboardMapping { Key = KeyCode.Alpha1,    Action = InputActionType.Select },       
            new KeyboardMapping { Key = KeyCode.Alpha2,    Action = InputActionType.Back },         
            new KeyboardMapping { Key = KeyCode.M,         Action = InputActionType.SwitchMode },   
            
            new KeyboardMapping { Key = KeyCode.Space,     Action = InputActionType.NavigateForward },
            new KeyboardMapping { Key = KeyCode.Backspace, Action = InputActionType.NavigateBack },
            new KeyboardMapping { Key = KeyCode.Escape,    Action = InputActionType.QuitApp },
            
            
        };
        
        [Header("Serial Port Button Mappings")]
        public List<SerialButtonMapping> SerialMappings = new()
        {
            new SerialButtonMapping { ButtonId = 1, Action = InputActionType.ChangeLanguage },
        };
        
        public Dictionary<KeyCode, InputActionType> GetKeyboardDictionary()
        {
            var dict = new Dictionary<KeyCode, InputActionType>();
            foreach (var mapping in KeyboardMappings)
                dict[mapping.Key] = mapping.Action;
            return dict;
        }
        
        public Dictionary<int, InputActionType> GetSerialDictionary()
        {
            var dict = new Dictionary<int, InputActionType>();
            foreach (var mapping in SerialMappings)
                dict[mapping.ButtonId] = mapping.Action;
            return dict;
        }
    }
}