using System;
using System.Collections.Generic;
using System.Linq;
using EasyLog.Configuration;
using EasyLog.Formatters;
using EasyLog.Models;

namespace EasyLog.Services
{
    /// <summary>
    /// Service de gestion de l'état en temps réel
    /// Fichier unique qui se met à jour en continu
    /// Contient l'état de tous les travaux de sauvegarde
    /// </summary>
    public class StateLogService
    {
        private readonly LogConfiguration _config;
        private readonly ILogFormatter _formatter;
        private readonly object _lockObject = new();
        private List<StateEntry> _states = new();

        public StateLogService(LogConfiguration config)
        {
            _config = config;
            _formatter = FormatterFactory.GetFormatter(config.LogFormat);
            EnsureDirectoriesExist();
            LoadState();
        }

        /// <summary>
        /// Met à jour l'état d'un travail
        /// Crée un nouvel état ou met à jour l'existant
        /// </summary>
        public void UpdateState(StateEntry state)
        {
            lock (_lockObject)
            {
                try
                {
                    // Chercher l'état existant par ID (pas par nom!)
                    var existingIndex = _states.FindIndex(s => s.Id == state.Id);
                    
                    if (existingIndex >= 0)
                    {
                        // Mettre à jour
                        _states[existingIndex] = state;
                    }
                    else
                    {
                        // Ajouter si c'est un nouvel ID
                        _states.Add(state);
                    }

                    // Mettre à jour l'horodatage
                    state.LastActionTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    
                    // Sauvegarder
                    SaveState();

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"?? Erreur lors de la mise à jour de l'état: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Met à jour plusieurs états
        /// </summary>
        public void UpdateStates(List<StateEntry> states)
        {
            foreach (var state in states)
            {
                UpdateState(state);
            }
        }

        /// <summary>
        /// Récupère l'état d'un travail
        /// </summary>
        public StateEntry? GetState(string backupName)
        {
            lock (_lockObject)
            {
                return _states.FirstOrDefault(s => s.Name == backupName);
            }
        }

        /// <summary>
        /// Récupère l'état d'un travail par ID unique
        /// </summary>
        public StateEntry? GetStateById(string stateId)
        {
            lock (_lockObject)
            {
                return _states.FirstOrDefault(s => s.Id == stateId);
            }
        }

        /// <summary>
        /// Récupère tous les états
        /// </summary>
        public List<StateEntry> GetAllStates()
        {
            lock (_lockObject)
            {
                return new List<StateEntry>(_states);
            }
        }

        /// <summary>
        /// Efface l'état d'un travail
        /// </summary>
        public void RemoveState(string backupName)
        {
            lock (_lockObject)
            {
                try
                {
                    _states.RemoveAll(s => s.Name == backupName);
                    SaveState();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"?? Erreur lors de la suppression de l'état: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Efface l'état d'un travail par WorkIndex
        /// ? Appelé quand on supprime un travail
        /// </summary>
        public void RemoveStateByWorkIndex(int workIndex)
        {
            lock (_lockObject)
            {
                try
                {
                    _states.RemoveAll(s => s.WorkIndex == workIndex);
                    SaveState();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"?? Erreur lors de la suppression de l'état: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Efface tous les états
        /// </summary>
        public void ClearAllStates()
        {
            lock (_lockObject)
            {
                try
                {
                    _states.Clear();
                    SaveState();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"?? Erreur lors de la suppression des états: {ex.Message}");
                }
            }
        }

        // ============ MÉTHODES PRIVÉES ============

        private void LoadState()
        {
            try
            {
                var stateFile = _config.GetStateFilePath();
                
                if (!File.Exists(stateFile))
                {
                    _states = new List<StateEntry>();
                    return;
                }

                var content = File.ReadAllText(stateFile);
                _states = _formatter.ParseStateEntries(content);
            }
            catch
            {
                _states = new List<StateEntry>();
            }
        }

        private void SaveState()
        {
            try
            {
                var stateFile = _config.GetStateFilePath();
                var content = _formatter.FormatStateEntries(_states);
                File.WriteAllText(stateFile, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"?? Erreur lors de la sauvegarde de l'état: {ex.Message}");
            }
        }

        private void EnsureDirectoriesExist()
        {
            if (_config.AutoCreateDirectories)
            {
                Directory.CreateDirectory(_config.LogDirectory);
            }
        }
    }
}
