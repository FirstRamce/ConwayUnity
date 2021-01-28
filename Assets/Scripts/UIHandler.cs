using UnityEngine;
using System.IO;
using conway.lib;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using CsvHelper;
using System.Globalization;

public class UIHandler : MonoBehaviour, NewIterationCompleteListener
{
    private Slider _simulationSpeedSlider;
    public static int DefaultGeneratedFieldSize = 10;
    public static int DefaultGeneratedProbability = 100;
    [SerializeField]
    private float FieldHeight = 0;
    Dictionary<CellCoords, LifeCell> LivingCells;
    private List<LifeCell> DisplayLifeCells;
    [SerializeField]
    private GameObject LifeCellPrefab;
    [SerializeField]
    private GameObject LifeCellContainer;
    private ConwayGame game;
    private float _autoSimulationSpeed = 1;
    private bool _autoSimulationActive = false;
    [SerializeField]
    private InputField _generateFieldSizeInput;
    [SerializeField]
    private InputField _generateProbabilityInput;
    [SerializeField]
    private InputField _importFileLocationInput;
    [SerializeField]
    private Text _displayInformationText;
    private bool _fieldRepetitionFound = false;
    [SerializeField]
    private Text _displayIteration;
    [SerializeField]
    private GameObject Canvas;
    private String _inputFileLocation;
    [SerializeField]
    private GameObject _inputFileContainer;
    private IEnumerable<PredefinedPosition> _predefinedPositions;

    // Start is called before the first frame update
    void Start()
    {
        DisplayLifeCells = new List<LifeCell>();
        
        _generateFieldSizeInput.text = DefaultGeneratedFieldSize.ToString();
        _generateProbabilityInput.text = DefaultGeneratedProbability.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("h") && Input.GetKey(KeyCode.LeftControl))
        {
            if (Canvas.activeSelf)
            {
                Canvas.SetActive(false);
            }
            else
            {
                Canvas.SetActive(true);
            }
        }
    }


    public void StartGameOfLifeSimulation(bool predefinedLifes)
    {
        this._displayInformationText.text = "";
        int fieldSize = 0;
        int generateProbability = 0;
        if(!int.TryParse(this._generateFieldSizeInput.text, out fieldSize) || !int.TryParse(this._generateProbabilityInput.text, out generateProbability))
        {
            this._displayInformationText.text = "Insert numbers for field size and probability. Default values set";
            fieldSize = DefaultGeneratedFieldSize;
            generateProbability = DefaultGeneratedProbability;
        }
        else if(fieldSize <= 0 || generateProbability <= 0)
        {
            this._displayInformationText.text = "invalid numbers in field size or probability. Default values set";
            fieldSize = DefaultGeneratedFieldSize;
            generateProbability = DefaultGeneratedProbability;
        }
        this.StopAllCoroutines();
        this._fieldRepetitionFound = false;
        this._autoSimulationActive = false;
        game = new ConwayGame(fieldSize);
        game.SubscribeAsIterationListener(this);
        if (predefinedLifes) 
        {
            NewCellIteration(game.RunPredefinedGame(this._predefinedPositions));
        }
        else
        {
            NewCellIteration(game.StartSimulation(fieldSize, generateProbability));
        }
        
    }

    public void NewCellIteration(Dictionary<CellCoords, Cell> newCells)
    {
        LivingCells = new Dictionary<CellCoords, LifeCell>(new CellCoordsComparer());
        LivingCells.Clear();
        List<LifeCell> removeCellsFromList = new List<LifeCell>();
        foreach (LifeCell cell in DisplayLifeCells)
        {
            Cell CurrentCell;
            bool continueLife = newCells.TryGetValue(new CellCoords(cell.x, cell.y), out CurrentCell);
            if (!continueLife)
            {
                Destroy(cell.gameObject);
                removeCellsFromList.Add(cell);
            }
            else
            {
                LivingCells.Add(new CellCoords(cell.x, cell.y), cell);
            }

        }
        foreach(LifeCell removeCell in removeCellsFromList)
        {
            DisplayLifeCells.Remove(removeCell);
        }
        removeCellsFromList.Clear();


        if (game.RepeatingFieldFound() && !this._fieldRepetitionFound)
        {
            this._autoSimulationSpeed = 1;
            if(this._simulationSpeedSlider != null)
            {
                _simulationSpeedSlider.value = 1;
            }
            this._fieldRepetitionFound = true;
            _displayInformationText.text = "Field Repetition found";
        }
        foreach (CellCoords coordinates in newCells.Keys)
        {
            if (!LivingCells.ContainsKey(coordinates))
            {
                GameObject createdCell = Instantiate(LifeCellPrefab, new Vector3(coordinates.x, FieldHeight, coordinates.y), Quaternion.identity, LifeCellContainer.transform) as GameObject;
                LifeCell createdLifeCell = createdCell.GetComponent<LifeCell>();
                createdLifeCell.x = coordinates.x;
                createdLifeCell.y = coordinates.y;
                DisplayLifeCells.Add(createdLifeCell);
            }
        }

        this._displayIteration.text = String.Format("Iteration: {0}", game.GetIterationNumber());
        
        if (_autoSimulationActive)
        {
            StartCoroutine(NextIterationDelay());
        }
        //coroutine if auto simulation
    }

    private IEnumerator NextIterationDelay()
    {
        yield return new WaitForSeconds(1 / this._autoSimulationSpeed);
        game.NextIteration();
    }

    public void AutoSimulationStart()
    {
        if (!_autoSimulationActive)
        {
            _autoSimulationActive = true;
            if(game != null)
            {
                game.NextIteration();
            }
            
        }
        
    }

    public void AutoSimulationStop()
    {
        this.StopAllCoroutines();
        if (_autoSimulationActive)
        {
            _autoSimulationActive = false;
        }
    }

    public void OnSimulationSpeedSliderValueChanged(Slider changedSlider)
    {
        if(this._simulationSpeedSlider == null)
        {
            this._simulationSpeedSlider = changedSlider;
        }
        this._autoSimulationSpeed = changedSlider.value;
    }

    public void NextIteration()
    {
        if (!_autoSimulationActive)
        {
            game.NextIteration();
        }
        
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void ToggleImportFileOptions()
    {
        _inputFileContainer.SetActive(!_inputFileContainer.activeSelf);
    }

    public void OpenPredefinedGame()
    {
        
        this._inputFileLocation = this._importFileLocationInput.text;
        Debug.Log($"Inputlocation: {_inputFileLocation}");
        bool invalidInput = false;
        if (this._inputFileLocation.Length > 1)
        {
            game = new ConwayGame(0);
            if (File.Exists(this._inputFileLocation))
            {
                StreamReader fileReader = new StreamReader(this._inputFileLocation);
                CsvReader csvPredefinedPositions = new CsvReader(fileReader, CultureInfo.InvariantCulture);
                if(csvPredefinedPositions != null)
                {
                    this._predefinedPositions = csvPredefinedPositions.GetRecords<PredefinedPosition>();
                    StartGameOfLifeSimulation(true);
                }
                fileReader.Close();
                this.ToggleImportFileOptions();
            }
            else
            {
                invalidInput = true;
            }
            
        }
        else
        {
            invalidInput = true;
        }

        if (invalidInput)
        {
            _displayInformationText.text = "invalid input / file";
        }
        
    }

}
