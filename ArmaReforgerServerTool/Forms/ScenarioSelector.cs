﻿/******************************************************************************
 * File Name:    ScenarioSelector.cs
 * Project:      Arma Reforger Dedicated Server Tool for Windows
 * Description:  This is the Scenario Selector Form
 * 
 * Author:       Bradley Newman
 ******************************************************************************/

using ReforgerServerApp.Utils;

namespace ReforgerServerApp
{
    public partial class ScenarioSelector : Form
    {
        private readonly Main    m_parentForm;
        private readonly Thread  m_getScenariosThread;
        private bool             m_getScenariosThreadRunning = true;
        private bool             m_getScenariosRequested     = false;

        public ScenarioSelector(Main parent)
        {
            InitializeComponent();
            PrintSelectedScenario();
            m_parentForm            = parent;
            m_getScenariosRequested = true;
            m_getScenariosThread    = new(new ThreadStart(DoGetScenarios));
            m_getScenariosThread.Start();
        }

        /// <summary>
        /// Get List of Stock Scenarios
        /// </summary>
        /// <returns>List of strings representing stock scenarios</returns>
        private static List<string> GetStockScenarios()
        {
            List<string> scenarios = new();
            const string campaignScen        = "{ECC61978EDCC2B5A}Missions/23_Campaign.conf";
            const string gmEdenScen          = "{59AD59368755F41A}Missions/21_GM_Eden.conf";
            const string arlandTutScen       = "{94FDA7451242150B}Missions/103_Arland_Tutorial.conf";
            const string gmArlandScen        = "{2BBBE828037C6F4B}Missions/22_GM_Arland.conf";
            const string campNthCentralScen  = "{C700DB41F0C546E1}Missions/23_Campaign_NorthCentral.conf";
            const string campSWCoastScen     = "{28802845ADA64D52}Missions/23_Campaign_SWCoast.conf";
            const string combatOpsScen       = "{DAA03C6E6099D50F}Missions/24_CombatOps.conf";
            const string campArlandScen      = "{C41618FD18E9D714}Missions/23_Campaign_Arland.conf";
            const string combatOpsEveronScen = "{DFAC5FABD11F2390}Missions/26_CombatOpsEveron.conf";
            scenarios.Add(campaignScen);
            scenarios.Add(gmEdenScen);
            scenarios.Add(arlandTutScen);
            scenarios.Add(gmArlandScen);
            scenarios.Add(campNthCentralScen);
            scenarios.Add(campSWCoastScen);
            scenarios.Add(combatOpsScen);
            scenarios.Add(campArlandScen);
            scenarios.Add(combatOpsEveronScen);
            return scenarios;
        }

        /// <summary>
        /// Retrieve Scenarios from Enabled mods
        /// </summary>
        private void GetScenarios()
        {
            try
            {
                List<string> scenarios = new();
                foreach (Mod m in m_parentForm.GetEnabledModsList().Items)
                {
                    scenarios.AddRange(Mod.GetScenariosForMod(m.GetModID()));
                }

                foreach (string scenId in scenarios)
                {
                    scenarioList.Invoke((MethodInvoker) (() => scenarioList.Items.Add(scenId)));
                }
            }

            catch (Exception ex)
            {
                Utilities.DisplayErrorMessage("An error occurred while loading scenarios.", ex.Message);
            }
        }

        private void DoGetScenarios()
        {
            while (m_getScenariosThreadRunning)
            {
                try
                {
                    if (m_getScenariosRequested)
                    {
                        if (reloadScenariosBtn.IsHandleCreated)
                        {
                            currentlySelectedLbl.Invoke((MethodInvoker) (() => currentlySelectedLbl.Text = "Fetching scenarios from the workshop..."));
                            m_getScenariosRequested = false;
                            reloadScenariosBtn.Invoke((MethodInvoker) (() => reloadScenariosBtn.Enabled = false));

                            foreach (string scen in GetStockScenarios())
                            {
                                scenarioList.Invoke((MethodInvoker) (() => scenarioList.Items.Add(scen)));
                            }

                            GetScenarios();
                            reloadScenariosBtn.Invoke((MethodInvoker) (() => reloadScenariosBtn.Enabled = true));
                            currentlySelectedLbl.Invoke((MethodInvoker) (() => currentlySelectedLbl.Text = Constants.SELECT_SCENARIO_STR));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utilities.DisplayErrorMessage("Scenario Fetch thread encountered an error.", ex.Message);
                }
            }
        }

        /// <summary>
        /// Handler for when the Reload Scenarios Button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReloadScenariosButtonClicked(object sender, EventArgs e)
        {
            scenarioList.Items.Clear();
            m_getScenariosRequested = true;
        }

        /// <summary>
        /// Handler for when Select Scenario Button is pressed. 
        /// It will set the Scenario ID in the Server Config then close the modal.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectScenarioButtonClicked(object sender, EventArgs e)
        {
            if (manualScenarioIdTextBox.Text != String.Empty)
            {
                ConfigurationManager.GetInstance().GetServerConfiguration().root.game.scenarioId = manualScenarioIdTextBox.Text;
            }
            else
            {
                if (scenarioList.SelectedItem != null)
                {
                    ConfigurationManager.GetInstance().GetServerConfiguration().root.game.scenarioId = scenarioList.SelectedItem.ToString();
                }
            }
            m_parentForm.RefreshLoadedScenario();
            this.Close();
        }

        /// <summary>
        /// Handler for when the Scenario List selected item changes to prevent the 
        /// ability to change to a null selection (nothing selected)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScenarioListSelectionChanged(object sender, EventArgs e)
        {
            if (scenarioList.SelectedItem == null)
            {
                selectScenarioBtn.Enabled = false;
            }
            else
            {
                selectScenarioBtn.Enabled = true;
            }
        }

        /// <summary>
        /// If the scenario ID is valid (not empty and not null), print the selected scenario, 
        /// otherwise prompt user to select a scenario from the list
        /// </summary>
        private void PrintSelectedScenario()
        {
            string scenId = ConfigurationManager.GetInstance().GetServerConfiguration().root.game.scenarioId;
            if (scenId != null && scenId != string.Empty)
            {
                currentlySelectedLbl.Text = $"{Constants.CURRENTLY_SELECTED_STR} {scenId}";
            }
            else
            {
                currentlySelectedLbl.Text = Constants.SELECT_SCENARIO_STR;
            }
        }

        /// <summary>
        /// Handler for when text changes in the Manual Scenario ID field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManualScenarioIDTextChanged(object sender, EventArgs e)
        {
            _ = manualScenarioIdTextBox.Text != string.Empty || scenarioList.Items.Count > 0 ?
                selectScenarioBtn.Enabled = true : selectScenarioBtn.Enabled = false;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            m_getScenariosThreadRunning = false;
        }
    }
}
