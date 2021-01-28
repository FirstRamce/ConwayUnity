
using CsvHelper;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace conway.lib
{
    public class ConwayGame
    {
        private readonly int _defaultProbability = 50;
        private readonly int _defaultIterations = 50;
        private int StarterSize;
        private Dictionary<CellCoords, Cell> _cells;
        private int Iteration = 0;
        private int RepeatCheckStarted = 0;
        private bool FieldRepeating = false;
        private int CheckAfterIterations = 100;
        private int CheckForIterations = 5;
        private List<NewIterationCompleteListener> IterationListeners;
        private List<Dictionary<CellCoords, Cell>> _repetitionList;

        public bool RepeatingFieldFound()
        {
            return this.FieldRepeating;
        }

        public int GetIterationNumber()
        {
            return this.Iteration;
        }

        public ConwayGame(int StarterSize)
        {
            
            this.StarterSize = StarterSize;
            _cells = new Dictionary<CellCoords, Cell>(new CellCoordsComparer());
            _repetitionList = new List<Dictionary<CellCoords, Cell>>();
            IterationListeners = new List<NewIterationCompleteListener>();
        }

        public void SubscribeAsIterationListener(NewIterationCompleteListener listener)
        {
            IterationListeners.Add(listener);
        }

        public void UnsubscribeAsIterationListener(NewIterationCompleteListener listener)
        {
            if (IterationListeners.Contains(listener))
            {
                IterationListeners.Remove(listener);
            }
            
        }

        public Dictionary<CellCoords, Cell> RunPredefinedGame(IEnumerable<PredefinedPosition> livingCells)
        {
            _cells.Clear();
            _repetitionList.Clear();
            setPredefinedCells(livingCells);
            return _cells;
        }

        private void setPredefinedCells(IEnumerable<PredefinedPosition> livingCells)
        {
            try
            {
                foreach (PredefinedPosition newCell in livingCells)
                {
                    if (!_cells.ContainsKey(new CellCoords(newCell.x, newCell.y)))
                    {
                        _cells.Add(new CellCoords(newCell.x, newCell.y), new Cell());
                    }

                }
            }
            catch(HeaderValidationException ex)
            {
                return;
            }
        }

        public Dictionary<CellCoords, Cell> StartSimulation(int fieldSize, int probability)
        {
            this.FieldRepeating = false;
            Iteration = 0;
            _repetitionList = new List<Dictionary<CellCoords, Cell>>();
            if (fieldSize != -1) StarterSize = fieldSize;
            if (probability == -1) probability = _defaultProbability;
            _cells.Clear();
            _repetitionList.Clear();
            GenerateCellsWithProbability(probability, StarterSize);
            return _cells;
        }

        public void NextIteration() {
            IterateSimulation();
            Iteration = Iteration + 1;
            if (Iteration % CheckAfterIterations == 0 && Iteration != 0)
            {
                RepeatCheckStarted = Iteration;
            }
            if (Iteration - RepeatCheckStarted <= CheckForIterations)
            {
                if (IsCurrentIterationRepetition())
                {
                    FieldRepeating = true;
                }
                AddDictionaryToCheckList();
            }
            else if (Iteration - RepeatCheckStarted == CheckForIterations + 1)
            {
                _repetitionList.Clear();
            }
            
            foreach (NewIterationCompleteListener listener in IterationListeners)
            {
                listener.NewCellIteration(_cells);
            }
            
        }

        private void AddDictionaryToCheckList()
        {
            Dictionary<CellCoords, Cell> copyDictionary = new Dictionary<CellCoords, Cell>(_cells.Count, _cells.Comparer);
            foreach (KeyValuePair<CellCoords, Cell> cellPair in _cells)
            {
                copyDictionary.Add(cellPair.Key, cellPair.Value);
            }

            this._repetitionList.Add(copyDictionary);
        }

        private Boolean IsCurrentIterationRepetition()
        {
            foreach (Dictionary<CellCoords, Cell> iterateDictionary in _repetitionList)
            {
                Boolean iterateDictionaryRepeated = true;
                if (iterateDictionary.Count == _cells.Count)
                {
                    foreach (CellCoords cellCoord in iterateDictionary.Keys)
                    {
                        Cell currentAliveCell;
                        if (!_cells.TryGetValue(cellCoord, out currentAliveCell))
                        {
                            iterateDictionaryRepeated = false;
                            break;
                        }
                    }
                    if (iterateDictionaryRepeated)
                    {
                        /*Console.WriteLine("FOUND DICTIONARY-------------------------");
                        PrintDictionary(iterateDictionary);
                        Console.WriteLine("CURRENT DICTIONARY-------------------------");
                        PrintDictionary(cells);*/
                        return true;
                    }
                }

            }
            return false;
        }

        private void PrintDictionary(Dictionary<CellCoords, Cell> dictionary)
        {
            foreach (KeyValuePair<CellCoords, Cell> cell in dictionary)
            {
                Console.WriteLine($"Alive: x({cell.Key.x}), y({cell.Key.y})");
            }
        }

        private void GenerateDefinedAmountOfCells(int AmountOfCells)
        {
            
            for (int i = 0; i < AmountOfCells; i++)
            {
                bool added = false;
                do
                {
                    var Random = new System.Random();
                    var x = Random.Next(StarterSize * -1, StarterSize + 1);
                    var y = Random.Next(StarterSize * -1, StarterSize + 1);
                    if(!_cells.ContainsKey(new CellCoords(x, y)))
                    {
                        added = true;
                        _cells.Add(new CellCoords(x, y), new Cell());
                    }
                } while (added == false);
            }
        }

        private void GenerateCellsWithProbability(int promilleMax, int fieldSize)
        {
            //because 0/0 is also a field
            fieldSize--;
            //Field size 1 generates a 2 * 2 field
            int negativStartPoint = 0;
            int positiveEndPoint = 0;
            if (fieldSize % 2 == 0)
            {
                negativStartPoint = fieldSize / 2 * -1;
                positiveEndPoint = fieldSize / 2;
            }
            else
            {
                negativStartPoint = fieldSize / 2 * -1;
                positiveEndPoint = fieldSize / 2 + 1;
            }
            Random Random = new System.Random();
            for (int x = negativStartPoint; x <= positiveEndPoint; x++)
            {
                for (int y = negativStartPoint; y <= positiveEndPoint; y++)
                {

                    int random = Random.Next(1001);
                    if (random <= promilleMax)
                    {
                        _cells.Add(new CellCoords(x, y), new Cell());
                    }
                }
            }
        }

        private void IterateSimulation()
        {
            var newCells = new Dictionary<CellCoords, Cell>(new CellCoordsComparer());
            foreach (System.Collections.Generic.KeyValuePair<conway.lib.CellCoords, conway.lib.Cell> cell in _cells)
            {
                for (int y = cell.Key.y - 1; y <= (cell.Key.y + 1); y++)
                {
                    for (int x = cell.Key.x - 1; x <= (cell.Key.x + 1); x++)
                    {
                        int Neighbours = GetAliveNeighbours(x, y);
                        Cell CurrentCell;
                        bool Exists = _cells.TryGetValue(new CellCoords(x, y), out CurrentCell);
                        Cell dummy = null;
                        switch (Neighbours)
                        {
                            case 2:
                                if (Exists)
                                {
                                    
                                    if(!newCells.TryGetValue(new CellCoords(x, y), out dummy))
                                    {
                                        newCells.Add(new CellCoords(x, y), CurrentCell);
                                    }
                                }
                                break;
                            case 3:
                                if (Exists == false)
                                {
                                    bool Added = false;
                                    if(!newCells.TryGetValue(new CellCoords(x, y), out dummy))
                                    {
                                        Added = true;
                                        newCells.Add(new CellCoords(x, y), CurrentCell);
                                    }
                                    
                                    if (Added)
                                    {
                                        //TODO: increase heat+
                                    }
                                }
                                else
                                {
                                    if (!newCells.TryGetValue(new CellCoords(x, y), out dummy))
                                    {
                                        newCells.Add(new CellCoords(x, y), CurrentCell);
                                    }
                                }
                                break;
                            default:
                                if (Exists)
                                {
                                    //TODO: decrease heat-
                                }
                                break;
                        }
                    }
                }
            }

            _cells = newCells;
        }

        private int GetAliveNeighbours(int CoordX, int CoordY)
        {
            int Neighbours = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 0 || y != 0)
                    {
                        Cell dummy;
                        bool alive;
                        alive = _cells.TryGetValue(new CellCoords(CoordX + x, CoordY + y), out dummy);
                        if (alive)
                        {
                            Neighbours++;
                        }
                    }
                }
            }
            return Neighbours;
        }
    }

    public interface NewIterationCompleteListener
    {
         void NewCellIteration(Dictionary<CellCoords, Cell> newCells);
    }

    public class PredefinedPosition
    {
        public int x { get; set; }
        public int y { get; set; }

        public PredefinedPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public PredefinedPosition()
        {

        }
    }
}