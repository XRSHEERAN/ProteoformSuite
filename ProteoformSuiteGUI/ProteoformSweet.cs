﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using ProteoformSuiteInternal;

namespace ProteoformSuite
{
    public partial class ProteoformSweet : Form
    {
        //  Initialize Forms START
        LoadResults loadResults = new LoadResults();
        RawExperimentalComponents rawExperimentalComponents = new RawExperimentalComponents();
        NeuCodePairs neuCodePairs = new NeuCodePairs();
        AggregatedProteoforms aggregatedProteoforms = new AggregatedProteoforms();
        TheoreticalDatabase theoreticalDatabase = new TheoreticalDatabase();
        ExperimentTheoreticalComparison experimentalTheoreticalComparison = new ExperimentTheoreticalComparison();
        ExperimentExperimentComparison experimentExperimentComparison = new ExperimentExperimentComparison();
        ProteoformFamilies proteoformFamilies = new ProteoformFamilies();
        TopDown topDown = new TopDown();
        Quantification quantification = new Quantification();
        ResultsSummary resultsSummary = new ResultsSummary();
        List<Form> forms;
        //  Initialize Forms END

        FolderBrowserDialog resultsFolderOpen = new FolderBrowserDialog();
        OpenFileDialog methodFileOpen = new OpenFileDialog();
        SaveFileDialog methodFileSave = new SaveFileDialog();
        SaveFileDialog saveDialog = new SaveFileDialog();

        Form current_form;

        public static bool run_when_form_loads = true;

        public ProteoformSweet()
        {
            InitializeComponent();
            InitializeForms();
            this.WindowState = FormWindowState.Maximized;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            showForm(loadResults);
            methodFileOpen.Filter = "Method TXT File (*.txt)| *.txt";
        }

        public void InitializeForms()
        {
            forms = new List<Form>(new Form[] {
                loadResults, rawExperimentalComponents, neuCodePairs, aggregatedProteoforms,
                theoreticalDatabase, experimentalTheoreticalComparison, experimentExperimentComparison,
                proteoformFamilies, quantification
            });
        }

        private void showForm(Form form)
        {
            form.MdiParent = this;
            form.Show();
            form.WindowState = FormWindowState.Maximized;
            current_form = form;
        }
        

        // RESULTS TOOL STRIP
        public void loadResultsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(loadResults); }
        private void rawExperimentalProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(rawExperimentalComponents);
            rawExperimentalComponents.load_raw_components();
        }
        private void neuCodeProteoformPairsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(neuCodePairs);
            neuCodePairs.display_neucode_pairs();
        }
        private void aggregatedProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(aggregatedProteoforms);
            if (run_when_form_loads) aggregatedProteoforms.aggregate_proteoforms();
        }
        private void theoreticalProteoformDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theoreticalDatabase.reload_database_list();
            showForm(theoreticalDatabase);
        }
        private void experimentTheoreticalComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(experimentalTheoreticalComparison);
            if (run_when_form_loads) experimentalTheoreticalComparison.compare_et();
        }
        private void experimentExperimentComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(experimentExperimentComparison);
            if (run_when_form_loads) experimentExperimentComparison.compare_ee();
        }
        private void proteoformFamilyAssignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(proteoformFamilies);
            proteoformFamilies.construct_families();
        }
        private void topdownResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(topDown);
            if (run_when_form_loads) topDown.load_topdown();
        }

        private void quantificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (run_when_form_loads) quantification.perform_calculations();
            quantification.initialize_every_time();
            showForm(quantification);
        }
        private void resultsSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resultsSummary.createResultsSummary();
            resultsSummary.displayResultsSummary();
            showForm(resultsSummary);
        }
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("printToolStripMenuItem_Click");
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        // METHOD TOOL STRIP
        private void saveMethodToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.methodFileSave.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
                saveMethod(methodFileSave.FileName);
        }

        private void saveMethod(string method_filename)
        {
            using (StreamWriter file = new StreamWriter(method_filename))
                file.WriteLine(SaveState.save_method());
        }

        private void loadSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            load_method();
        }

        private bool load_method()
        {
            DialogResult dr = this.methodFileOpen.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string method_filename = methodFileOpen.FileName;
                ResultsSummary.loadDescription = method_filename;
                SaveState.open_method(File.ReadAllLines(method_filename));
                return true;
            }
            return false;
        }

        private void loadRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Lollipop.input_files.Count == 0)
            {
                MessageBox.Show("Please load in deconvolution result files in order to use load and run.");
                return;
            }
            var result = MessageBox.Show("Choose a method file.", "Method Load and Run", MessageBoxButtons.OKCancel);
            if (result == DialogResult.Cancel) return;

            if (!load_method()) return;
            MessageBox.Show("Successfully loaded method. Will run the method now.");

            if (full_run()) MessageBox.Show("Successfully ran method. Feel free to explore using the Results menu.");
            else MessageBox.Show("Method did not successfully run.");
         }

        public bool full_run()
        {
            clear_lists();
            if (Lollipop.get_files(Lollipop.input_files, Purpose.PtmList).Count() <= 0 && Lollipop.get_files(Lollipop.input_files, Purpose.ProteinDatabase).Count() <= 0)
            {
                MessageBox.Show("Please list at least one protein database and at least one PTM list.");
                return false;
            }
            this.Cursor = Cursors.WaitCursor;
            rawExperimentalComponents.load_raw_components();
            aggregatedProteoforms.aggregate_proteoforms();
            theoreticalDatabase.make_databases();
            Lollipop.make_et_relationships();
            Lollipop.make_ee_relationships();
            if (Lollipop.neucode_labeled) proteoformFamilies.construct_families();
            quantification.perform_calculations();
            prepare_figures_and_tables();
            this.enable_neuCodeProteoformPairsToolStripMenuItem(Lollipop.neucode_labeled);
            this.Cursor = Cursors.Default;
            return true;
        }

        private void prepare_figures_and_tables()
        {
            Parallel.Invoke
            (
                () => rawExperimentalComponents.FillRawExpComponentsTable(),
                () => aggregatedProteoforms.FillAggregatesTable(),
                () => theoreticalDatabase.FillDataBaseTable("Target"),
                () => experimentalTheoreticalComparison.FillTablesAndCharts(),
                () => experimentExperimentComparison.FillTablesAndCharts()
            );
            if (Lollipop.neucode_labeled) neuCodePairs.GraphNeuCodePairs();
        }
    

        // MISCELLANEOUS
        public void clear_lists()
        {
            Lollipop.raw_experimental_components.Clear();
            Lollipop.raw_neucode_pairs.Clear();
            Lollipop.proteoform_community.experimental_proteoforms = new ExperimentalProteoform[0];
            Lollipop.proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            Lollipop.et_relations.Clear();
            Lollipop.et_peaks.Clear();
            Lollipop.ee_relations.Clear();
            Lollipop.ee_peaks.Clear();
            Lollipop.proteoform_community.families.Clear();
            Lollipop.ed_relations.Clear();
            Lollipop.proteoform_community.relations_in_peaks.Clear();
            Lollipop.proteoform_community.delta_mass_peaks.Clear();
            Lollipop.ef_relations.Clear();
            Lollipop.proteoform_community.decoy_proteoforms.Clear();
        }

        public void enable_neuCodeProteoformPairsToolStripMenuItem(bool setting)
        {
            neuCodeProteoformPairsToolStripMenuItem.Enabled = setting;
        }

        public void display_resultsMenu()
        {
            resultsToolStripMenuItem.ShowDropDown();
        }

        public void display_methodMenu()
        {
            runMethodToolStripMenuItem.ShowDropDown();
        }

        private void exportTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            export_table();
        }

        private void export_table()
        {
            List<DataGridView> dgvs = new List<DataGridView>();
            if (current_form == rawExperimentalComponents)
            {
                dgvs.Add(rawExperimentalComponents.GetDGV());
                SaveExcelFile(dgvs, "raw_experimental_components_table.xlsx");
            }
            if (current_form == neuCodePairs)
            {
                dgvs.Add(neuCodePairs.GetDGV());
                SaveExcelFile(dgvs, "neucode_pairs_table.xlsx");
            }
            if (current_form == aggregatedProteoforms)
            {
                dgvs.Add(aggregatedProteoforms.GetDGV());
                SaveExcelFile(dgvs, "aggregated_proteoforms_table.xlsx");
           }
            if (current_form == theoreticalDatabase)
            {
                dgvs.Add(theoreticalDatabase.GetDGV());
                SaveExcelFile(dgvs, "theoretical_database_table.xlsx");
            }
            if ( current_form == experimentalTheoreticalComparison)
            {
                dgvs.Add(experimentalTheoreticalComparison.GetETPeaksDGV());
                dgvs.Add(experimentalTheoreticalComparison.GetETRelationsDGV());
                SaveExcelFile(dgvs, "experimental_theoretical_comparison_table.xlsx");
            }
            if ( current_form == experimentExperimentComparison)
            {
                dgvs.Add(experimentExperimentComparison.GetEEPeaksDGV());
                dgvs.Add(experimentExperimentComparison.GetEERelationDGV());
                SaveExcelFile(dgvs, "experiment_experiment_comparison_table.xlsx");
            }
            if (current_form == proteoformFamilies)
            {
                dgvs.Add(proteoformFamilies.GetDGV());
                SaveExcelFile(dgvs, "proteoform_families_table.xlsx");
            }
            if (current_form == quantification)
            {
                dgvs.Add(quantification.Get_GoTerms_DGV());
                dgvs.Add(quantification.Get_quant_results_DGV());
                SaveExcelFile(dgvs, "quantification_table.xlsx");
            }
        }

        public void SaveExcelFile(List<DataGridView> dgvs, string filename)
        {
            saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            saveDialog.FileName = filename;
            DialogResult dr = this.saveDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                DGVExcelWriter writer = new DGVExcelWriter();
                writer.ExportToExcel(dgvs, saveDialog.FileName);
                MessageBox.Show("Successfully exported table.");
            }
            else return; 
        }


    }
}
