using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;




/*
 * This is an example of a command.
 * These can be static, or non-static but non-static you must target the gameobject that contains the component
 * with the command within it to execute it.
 * 
[SteamPunkConsoleCommand(command = "TestCommand", info = "This is a test command, it used to test three parameters.")]
static public void TestCommand(int valueInt, float valueFloat, string valueString)
{
    WriteLine("TEST WORKS!");
}
*/


public class SteamPunkConsoleCommand : System.Attribute
{
    public string info = "";
    public string command = "";
}

public class SteamPunkCommandEntry
{
    public SteamPunkConsoleCommand macro;
    public MethodInfo method;
}

public class SteamPunkConsole : MonoBehaviour
{
    GameObject canvas;
    public Text textTemplate;

    public GameObject consoleContent;
    public GameObject autoCompleteContainer;
    public ScrollRect scrollRect;
    public Text targetDisplay;
    public InputField consoleInput;
    public KeyCode boundKey = KeyCode.BackQuote;
    public GameObject hierachyContainer;
    public GameObject settingsContainer;
    public SteamPunkHierachy hierachy;
    public SteamPunkSettings settings;
    public SteampunkAutoComplete autoComplete;

    public int textBlockSize = 1024;

    /// <summary>
    /// If there is no event system when the console wakes, it will attach one to itself.
    /// if you wish to 'wait' till an event system arrives, set this false.
    /// </summary>
    public bool generateMissingEventSystem = true;

    Text currentTextBlock;

    static public SteamPunkConsole Instance { get; private set; }

    Dictionary<string, SteamPunkCommandEntry> commands = new Dictionary<string, SteamPunkCommandEntry>();
    List<SteamPunkCommandEntry> commandList = new List<SteamPunkCommandEntry>();

    GameObject targetGameObject = null;
    Component[] targetComponents = null;

    List<string> commandHistory = new List<string>(128);
    int currentHistory = 0;

    List<string> parts = new List<string>(128);

    /// <summary>
    /// If you wish to allow the game to run while the console is open, but block input from
    /// the game while the console is open, this static boolean is provided so you can block
    /// input in your own components.
    /// </summary>
    static public bool IsConsoleOpen
    {
        get
        {
            if (Instance == null) return false;
            return Instance.canvas.activeSelf;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        EventSystem[] eventSystems = GameObject.FindObjectsOfType<EventSystem>();
        if (eventSystems.Length == 0 && generateMissingEventSystem)
        {
            gameObject.AddComponent<EventSystem>();
            gameObject.AddComponent<StandaloneInputModule>();
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        consoleInput.onValidateInput += OnValidateInput;
        consoleInput.onValueChanged.AddListener(OnValueChanged);
        currentTextBlock = textTemplate;

        canvas = transform.Find("Canvas").gameObject;
        canvas.SetActive(false);

        hierachy.onSelectTarget += OnHierachySelect;
        settings.onSettingChanged += OnSettingChanged;
        autoComplete.onClickedSuggestion += OnClickedSuggestion;
    }

    void OnClickedSuggestion(string suggestion)
    {
        if (suggestion != null)
            consoleInput.text = suggestion;
    }

    void OnSettingChanged(SteamPunkSetting setting, bool val)
    {
        switch (setting)
        {
            case SteamPunkSetting.PauseWhileOpen:
                if (IsConsoleOpen)
                    Time.timeScale = val ? 0 : 1;
                break;
            case SteamPunkSetting.ShowUnityLog:
                if (val)
                    Application.logMessageReceived += logMessageReceived;
                else
                    Application.logMessageReceived -= logMessageReceived;
                break;
        }
    }

    private void logMessageReceived(string condition, string stackTrace, LogType type)
    {
        WriteLine(condition);
    }

    void OnHierachySelect(GameObject gameObject)
    {
        targetGameObject = gameObject;
        if (targetGameObject != null)
        {
            targetDisplay.text = "[" + gameObject.name + "]";
            targetComponents = targetGameObject.GetComponents<Component>();
        }
        else
            targetComponents = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        System.Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        for (int i = 0; i < types.Length; i++)
        {
            MethodInfo[] methods = types[i].GetMethods();
            for (int m = 0; m < methods.Length; m++)
            {
                object[] attributes = methods[m].GetCustomAttributes(typeof(SteamPunkConsoleCommand), false);

                for (int a = 0; a < attributes.Length; a++)
                {
                    SteamPunkConsoleCommand macro = (SteamPunkConsoleCommand)attributes[a];
                    if (macro.command != "")
                    {
                        SteamPunkCommandEntry entry = new SteamPunkCommandEntry() { macro = macro, method = methods[m] };
                        commands.Add(macro.command.ToLower(), entry);
                        commandList.Add(entry);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current == null)
            return;

        if (Input.GetKeyDown(boundKey))
        {
            canvas.SetActive(!canvas.activeSelf);
            if (settings.GetSettingValue(SteamPunkSetting.PauseWhileOpen))
                Time.timeScale = canvas.activeSelf ? 0 : 1;

            if (canvas.activeSelf)
            {
                consoleInput.ActivateInputField();
                hierachy.OnCloseHierachy();
            }
            else if (EventSystem.current.currentSelectedGameObject == consoleInput.gameObject)
                EventSystem.current.SetSelectedGameObject(null);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (commandHistory.Count == 0)
                return;

            currentHistory++;
            currentHistory = Mathf.Clamp(currentHistory, -1, commandHistory.Count - 1);
            consoleInput.text = commandHistory[currentHistory];
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (commandHistory.Count == 0)
                return;

            currentHistory--;
            currentHistory = Mathf.Clamp(currentHistory, -1, commandHistory.Count - 1);
            if (currentHistory == -1)
                consoleInput.text = "";
            else
                consoleInput.text = commandHistory[currentHistory];
        }

        if (EventSystem.current.currentSelectedGameObject == consoleInput.gameObject)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                string suggestion = autoComplete.Suggestion;
                if (suggestion != null)
                    consoleInput.text = suggestion;
            }
            else if (Input.GetKeyDown(KeyCode.Return))
                RunCommand(consoleInput.text);
        }
    }

    public void OnToggleHierachy()
    {
        if (!hierachyContainer.activeSelf)
            hierachy.ToggleHierachy();
    }

    public void OnToggleSettings()
    {
        settingsContainer.SetActive(!settingsContainer.activeSelf);
    }

    public char OnValidateInput(string input, int charIndex, char charToValidate)
    {
        if (!chartoKeycode.ContainsKey(charToValidate))
        {
            Debug.Log("INVALID INPUT: " + charToValidate);
            return '\0';
        }

        if (chartoKeycode[charToValidate] == boundKey)
            return '\0';

        return charToValidate;
    }

    void OnValueChanged(string input)
    {
        autoComplete.UpdateList(input, targetGameObject, ref targetComponents, commandList);
    }

    public void OnConsoleEnd(string str)
    {
        consoleInput.ActivateInputField();
        currentHistory = -1;
    }

    void RunCommand(string rawCommand)
    {
        bool willScroll = scrollRect.verticalNormalizedPosition == 0;

        commandHistory.Insert(0, rawCommand);
        consoleInput.text = "";

        SteamPunkCommandEntry entry = GetCommandEntry(rawCommand);
        if (entry == null)
            WriteLine("Error: Invalid Command.");
        else
        {
            object[] parameters = ParseParameters(entry, rawCommand);
            if (parameters != null)
                ExecuteCommand(entry, parameters);
        }


    }

    SteamPunkCommandEntry GetCommandEntry(string rawCommand)
    {
        if (!rawCommand.Contains(" "))
        {
            SteamPunkCommandEntry entry = null;
            commands.TryGetValue(rawCommand.ToLower(), out entry);
            return entry;
        }
        else
        {
            string firstSegment = rawCommand.Substring(0, rawCommand.IndexOf(' '));

            SteamPunkCommandEntry entry = null;
            commands.TryGetValue(firstSegment.ToLower(), out entry);

            return entry;
        }
    }

    object[] ParseParameters(SteamPunkCommandEntry cmd, string rawCommand)
    {
        string currentCommand = rawCommand;
        parts.Clear();

        while (currentCommand.Length != 0)
        {
            int spaceIndex = currentCommand.IndexOf(" ");
            int literalIndex = currentCommand.IndexOf('\'');
            if (literalIndex == -1)
                literalIndex = int.MaxValue;


            if (spaceIndex != -1)
            {
                int nextSpaceIndex = currentCommand.Substring(spaceIndex).IndexOf(" ");
                if (nextSpaceIndex == spaceIndex + 1)
                {
                    currentCommand = currentCommand.Substring(nextSpaceIndex + 1);
                    continue;
                }
            }

            if (spaceIndex < literalIndex || (spaceIndex == -1 && literalIndex == -1))
            {
                int nextIndex = currentCommand.IndexOf(" ");
                if (nextIndex != -1)
                {
                    string subString = currentCommand.Substring(0, nextIndex);
                    parts.Add(subString);

                    currentCommand = currentCommand.Substring(nextIndex + 1);
                }
                else
                {
                    parts.Add(currentCommand);
                    currentCommand = "";
                }
            }
            else if (literalIndex != -1) // found literal parse.
            {
                string subString = currentCommand.Substring(literalIndex + 1);
                int endIndex = subString.IndexOf("'");

                if (endIndex == -1) // invalid endIndex.
                {
                    WriteLine("Error. incorrect string literal.");
                    return null;
                }

                string literalString = subString.Substring(0, endIndex);
                parts.Add(literalString);

                if (subString.Length < endIndex + 1 + 1)
                    currentCommand = subString.Substring(endIndex + 1);
                else
                    currentCommand = subString.Substring(endIndex + 1 + 1);
            }

        }

        object[] parameters = new object[parts.Count - 1];
        ParameterInfo[] paramInfo = cmd.method.GetParameters();
        if (paramInfo.Length != parameters.Length)
        {
            WriteLine("Error. Incorrect parameters count.");
            return null;
        }

        for (int i = 0; i < parameters.Length; i++)
        {
            if (paramInfo[i].ParameterType == typeof(object))
                parameters[i] = ParseObjectType(parts[i + 1]);
            else if (paramInfo[i].ParameterType == typeof(string))
                parameters[i] = parts[i + 1];
            else if (paramInfo[i].ParameterType == typeof(bool))
            {
                string text = parts[i + 1];
                if (text.ToLower() == "false")
                    parameters[i] = false;
                else if (text.ToLower() == "true")
                    parameters[i] = true;
            }
            else if (paramInfo[i].ParameterType == typeof(int))
            {
                int intParam = 0;
                if (int.TryParse(parts[i + 1], out intParam))
                    parameters[i] = intParam;
                else
                {
                    WriteLine("Error. Invalid parameter. expected int received " + parts[i + 1]);
                    return null;
                }
            }
            else if (paramInfo[i].ParameterType == typeof(float))
            {
                float floatParam = 0;
                if (float.TryParse(parts[i + 1], out floatParam))
                    parameters[i] = floatParam;
                else
                {
                    WriteLine("Error. Invalid parameter. expected float received " + parts[i + 1]);
                    return null;
                }
            }
        }
        return parameters;
    }

    object ParseObjectType(string parameter)
    {
        float floatParse = 0;
        int intParse = 0;

        if (parameter.ToLower() == "false")
            return false;
        else if (parameter.ToLower() == "true")
            return true;
        else if (int.TryParse(parameter, out intParse))
            return intParse;
        else if (float.TryParse(parameter, out floatParse))
            return floatParse;

        return parameter;
    }

    void ExecuteCommand(SteamPunkCommandEntry cmd, object[] parameters)
    {
        if (cmd.method.IsStatic)
            cmd.method.Invoke(null, parameters);
        else 
        {
            Component component = null;
            if (targetGameObject != null)
                component = targetGameObject.GetComponent(cmd.method.DeclaringType);

            if (targetGameObject != null && component != null)
                cmd.method.Invoke(component, parameters);
            else
                WriteLine("Error: [" + cmd.macro.command + "] requires a target with the component [" + cmd.method.DeclaringType + "]");
        }
    }

    // static and public can be used where ever else to print things to the log.
    public static void WriteLine(string text)
    {
        bool willScroll = Instance.scrollRect.verticalNormalizedPosition == 0;

        if (Instance.currentTextBlock.text.Length >= Instance.textBlockSize)
        {
            Instance.currentTextBlock = GameObject.Instantiate(Instance.textTemplate, Instance.textTemplate.transform.parent);
            Instance.currentTextBlock.text = "";
        }
        
        Instance.currentTextBlock.text += "\n" + text;

        if (willScroll) // if we were scrolled to the bottom before the text was added, refresh the canvas to and scroll the rest of the way.
        {
            Canvas.ForceUpdateCanvases(); // might be overkill, but solves the problem!
            Instance.scrollRect.verticalNormalizedPosition = 0;
        }
    }

    public static void ClearLog()
    {
        foreach (Transform child in Instance.consoleContent.transform)
        {
            if (child != Instance.textTemplate.transform)
                GameObject.Destroy(child.gameObject);
        }

        Instance.textTemplate.text = "Log Cleared.";
    }

    [SteamPunkConsoleCommand(command = "Help", info = "Displays a list of all commands.")]
    static public void HelpCommand()
    {
        WriteLine("Commands:");
        foreach (KeyValuePair<string, SteamPunkCommandEntry> command in Instance.commands)
        {
            ParameterInfo[] paramInfo = command.Value.method.GetParameters();

            if (paramInfo.Length != 0)
            {
                string paramList = "(";
                for (int i = 0; i < paramInfo.Length; i++)
                {
                    paramList += paramInfo[i].ParameterType;
                    if (i + 1 != paramInfo.Length)
                        paramList += ",";
                }
                paramList += ")";
                WriteLine("[" + command.Key + "] " + paramList);
            }
            else
                WriteLine("[" + command.Key + "] ");

            WriteLine("   " + command.Value.macro.info);
        }
    }

    [SteamPunkConsoleCommand(command = "Echo", info = "Echos the following text into the log.")]
    static public void EchoCommand(string text)
    {
        WriteLine(text);
    }

    [SteamPunkConsoleCommand(command = "Clear", info = "Clears all the text from the log.")]
    static public void ClearCommand()
    {
        ClearLog();
    }

    [SteamPunkConsoleCommand(command = "SendMessage",
    info = "Sends a message to a gameobject in the same way as unity. (NOTE: Use SendMessageParam if you want to include a parameter.)")]
    static public void SendMessageCMD(string message)
    {
        if (Instance.targetGameObject == null)
            WriteLine("Error: Requires a target gameobject to SendMessage.");
        else
            Instance.targetGameObject.SendMessage(message);
    }

    [SteamPunkConsoleCommand(command = "SendMessageParam", 
        info = "Sends a message to a gameobject in the same way as unity. (NOTE: Use SendMessage if you wish to not include a parameter. )")]
    static public void SendMessageCMD(string message, object parameter)
    {
        if (Instance.targetGameObject == null)
            WriteLine("Error: Requires a target gameobject to SendMessage.");
        else
            Instance.targetGameObject.SendMessage(message, parameter);
    }

    [SteamPunkConsoleCommand(command = "TestParams")]
    static public void TestParams(string testString, int testInt, float testFloat, bool testBool)
    {
        WriteLine("testString: " + testString);
        WriteLine("testInt: " + testInt);
        WriteLine("testFloat: " + testFloat);
        WriteLine("testBool: " + testBool);
    }


    static Dictionary<char, KeyCode> chartoKeycode = new Dictionary<char, KeyCode>()
    {
      //Lower Case Letters
      {'a', KeyCode.A},
      {'b', KeyCode.B},
      {'c', KeyCode.C},
      {'d', KeyCode.D},
      {'e', KeyCode.E},
      {'f', KeyCode.F},
      {'g', KeyCode.G},
      {'h', KeyCode.H},
      {'i', KeyCode.I},
      {'j', KeyCode.J},
      {'k', KeyCode.K},
      {'l', KeyCode.L},
      {'m', KeyCode.M},
      {'n', KeyCode.N},
      {'o', KeyCode.O},
      {'p', KeyCode.P},
      {'q', KeyCode.Q},
      {'r', KeyCode.R},
      {'s', KeyCode.S},
      {'t', KeyCode.T},
      {'u', KeyCode.U},
      {'v', KeyCode.V},
      {'w', KeyCode.W},
      {'x', KeyCode.X},
      {'y', KeyCode.Y},
      {'z', KeyCode.Z},

      //Upper Case Letters
      {'A', KeyCode.A},
      {'B', KeyCode.B},
      {'C', KeyCode.C},
      {'D', KeyCode.D},
      {'E', KeyCode.E},
      {'F', KeyCode.F},
      {'G', KeyCode.G},
      {'H', KeyCode.H},
      {'I', KeyCode.I},
      {'J', KeyCode.J},
      {'K', KeyCode.K},
      {'L', KeyCode.L},
      {'M', KeyCode.M},
      {'N', KeyCode.N},
      {'O', KeyCode.O},
      {'P', KeyCode.P},
      {'Q', KeyCode.Q},
      {'R', KeyCode.R},
      {'S', KeyCode.S},
      {'T', KeyCode.T},
      {'U', KeyCode.U},
      {'V', KeyCode.V},
      {'W', KeyCode.W},
      {'X', KeyCode.X},
      {'Y', KeyCode.Y},
      {'Z', KeyCode.Z},


  
      //KeyPad Numbers
      {'1', KeyCode.Keypad1},
      {'2', KeyCode.Keypad2},
      {'3', KeyCode.Keypad3},
      {'4', KeyCode.Keypad4},
      {'5', KeyCode.Keypad5},
      {'6', KeyCode.Keypad6},
      {'7', KeyCode.Keypad7},
      {'8', KeyCode.Keypad8},
      {'9', KeyCode.Keypad9},
      {'0', KeyCode.Keypad0},
  
      //Other Symbols
      {' ', KeyCode.Space},
      {'!', KeyCode.Exclaim}, //1
      {'"', KeyCode.DoubleQuote},
      {'#', KeyCode.Hash}, //3
      {'$', KeyCode.Dollar}, //4
      {'&', KeyCode.Ampersand}, //7
      {'\'', KeyCode.Quote}, //remember the special forward slash rule... this isnt wrong
      {'(', KeyCode.LeftParen}, //9
      {')', KeyCode.RightParen}, //0
      {'*', KeyCode.Asterisk}, //8
      {'+', KeyCode.Plus},
      {',', KeyCode.Comma},
      {'-', KeyCode.Minus},
      {'.', KeyCode.Period},
      {'/', KeyCode.Slash},
      {':', KeyCode.Colon},
      {';', KeyCode.Semicolon},
      {'<', KeyCode.Less},
      {'=', KeyCode.Equals},
      {'>', KeyCode.Greater},
      {'?', KeyCode.Question},
      {'@', KeyCode.At}, //2
      {'[', KeyCode.LeftBracket},
      {'\\', KeyCode.Backslash}, //remember the special forward slash rule... this isnt wrong
      {']', KeyCode.RightBracket},
      {'^', KeyCode.Caret}, //6
      {'_', KeyCode.Underscore},
      {'`', KeyCode.BackQuote},
    };
}



/*
 * pixel art ideas.
 * 
 * WildLife
 * Undead
 * Pirate
 * Cult - Occult
 * Wizards
 * Hats
 * Frankensteins Monster
 * Lizards
 * Mechanical
 * Particle Effects
 * Vehicle
 * Space
 * Potato
 */
