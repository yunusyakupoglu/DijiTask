using BL.IServices;
using OL;
using System.Configuration;
using System.Timers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace UI
{
    public partial class Form1 : Form
    {
        private readonly IEmployeeService _employeeService;
        private readonly IMachineService _machineService;
        private readonly IShiftService _shiftService;
        int selectedEmployeeId;
        int selectedMachineId;
        int selectedShiftId;
        private System.Timers.Timer _statusTimer; // Timer nesnesi

        public Form1(IEmployeeService employeeService, IMachineService machineService, IShiftService shiftService)
        {
            InitializeComponent();
            _employeeService = employeeService;
            _machineService = machineService;
            _shiftService = shiftService;
        }


        private async void Form1_Load(object sender, EventArgs e)
        {
            var cs = LoadConnectionStringParameters();

            using (var dbContext = new DAL.AppDbContext())
            {
                bool isDatabaseExists = dbContext.Database.CanConnect();

                if (isDatabaseExists)
                {
                    dataGridView1.DataSource = await _employeeService.GetAllAsync();
                    dataGridView2.DataSource = await _machineService.GetAllAsync();
                    dataGridView3.DataSource = await _shiftService.GetAllShiftsAsync();
                    cmbEmployees.DataSource = await _employeeService.GetAllEmployeeNamesAsync();
                    cmbMachines.DataSource = await _machineService.GetAllMachineNamesAsync();
                    dataGridView3.Columns["Id"].Visible = false;
                    dataGridView3.Columns["ShiftId"].Visible = false;
                    dataGridView2.Columns["Id"].Visible = false;
                    dataGridView1.Columns["Id"].Visible = false;
                    dataGridView3.Columns["EmployeeId"].Visible = false;
                    dataGridView3.Columns["MachineId"].Visible = false;

                    dataGridView1.Columns["EmployeeName"].HeaderText = "Ad Soyad";
                    dataGridView2.Columns["MachineName"].HeaderText = "Makine";
                    dataGridView3.Columns["ShiftDate"].HeaderText = "Tarih";
                    dataGridView3.Columns["Employee"].HeaderText = "Ad Soyad";
                    dataGridView3.Columns["Machine"].HeaderText = "Makine";
                }
                else if (!isDatabaseExists)
                {
                    tabControl1.SelectedTab = tabPage4;
                }


            }


        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Employees employee = new Employees();
            employee.EmployeeName = txtNameSurname.Text;

            await _employeeService.CreateAsync(employee);
            dataGridView1.DataSource = await _employeeService.GetAllAsync();
            cmbEmployees.DataSource = await _employeeService.GetAllEmployeeNamesAsync();
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private async void btnAddMachine_Click(object sender, EventArgs e)
        {
            Machines machine = new Machines();
            machine.MachineName = txtMachineName.Text;

            await _machineService.CreateAsync(machine);
            dataGridView2.DataSource = await _machineService.GetAllAsync();
            cmbMachines.DataSource = await _machineService.GetAllMachineNamesAsync();
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            var selectedMachine = await _machineService.GetByFilterAsync(x => x.MachineName == cmbMachines.SelectedValue);
            var selectedEmployee = await _employeeService.GetByFilterAsync(x => x.EmployeeName == cmbEmployees.SelectedValue);

            // �nce ayn� personele ayn� g�n i�inde atanan i�leri kontrol edin
            bool isDuplicate = false;
            var existingShifts = await _shiftService.GetAllShiftsAsync();
            foreach (var existingShift in existingShifts)
            {
                if (existingShift.EmployeeId == selectedEmployee.Id && existingShift.ShiftDate == dateTimePicker1.Value.Date)
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (isDuplicate)
            {
                // E�er personele ayn� g�n i�inde i� atanm��sa bu durumu i�leyin (�rne�in, bir hata mesaj� g�sterin).
                MessageBox.Show("Bu personele ayn� g�n i�inde iki farkl� i� atanamaz.");
            }
            else
            {
                // E�er ayn� personele ayn� g�n i�inde i� atanmam��sa yeni vardiya olu�turun
                Shifts shift = new Shifts();
                shift.ShiftDate = dateTimePicker1.Value.Date;
                shift.MachineId = selectedMachine.Id;
                shift.EmployeeId = selectedEmployee.Id;

                await _shiftService.CreateAsync(shift);
                dataGridView3.DataSource = await _shiftService.GetAllShiftsAsync();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var workbook = _shiftService.ExportWeeklyShiftDataToExcelUsingClosedXML();
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel Dosyas�|*.xlsx";
                saveFileDialog.Title = "Excel Dosyas�n� Kaydet";
                saveFileDialog.FileName = "WeeklyShiftData.xlsx"; // Varsay�lan dosya ad�

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string excelFilePath = saveFileDialog.FileName;
                    workbook.SaveAs(excelFilePath);
                    MessageBox.Show("Excel dosyas� ba�ar�yla kaydedildi. Dosya konumu: " + excelFilePath);
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var foundData = await _employeeService.GetByFilterAsync(x => x.Id == selectedEmployeeId);
            if (foundData != null)
            {
                Employees employee = foundData;
                employee.EmployeeName = txtNameSurname.Text;
                var empresult = await _employeeService.UpdateAsync(employee);
                if (empresult)
                {
                    dataGridView1.DataSource = await _employeeService.GetAllAsync();
                    dataGridView3.DataSource = await _shiftService.GetAllShiftsAsync();
                    MessageBox.Show("Personel verisi g�ncellendi.");
                }
            }
            else
                MessageBox.Show("Personel verisi bulunamad�.");
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                txtNameSurname.Text = selectedRow.Cells[0].Value.ToString();
                selectedEmployeeId = Convert.ToInt32(selectedRow.Cells[1].Value.ToString());
            }
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView2.SelectedRows[0];
                txtMachineName.Text = selectedRow.Cells[0].Value.ToString();
                selectedMachineId = Convert.ToInt32(selectedRow.Cells[1].Value.ToString());
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {

            var foundData = await _machineService.GetByFilterAsync(x => x.Id == selectedMachineId);
            if (foundData != null)
            {
                Machines machine = foundData;
                machine.MachineName = txtMachineName.Text;
                var macresult = await _machineService.UpdateAsync(machine);
                if (macresult)
                {
                    dataGridView2.DataSource = await _machineService.GetAllAsync();
                    dataGridView3.DataSource = await _shiftService.GetAllShiftsAsync();
                    MessageBox.Show("Makine verisi g�ncellendi.");
                }
            }
            else
                MessageBox.Show("Makine verisi bulunamad�.");
        }

        private void dataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView3.SelectedRows[0];
                cmbEmployees.Text = selectedRow.Cells[4].Value.ToString();
                cmbMachines.Text = selectedRow.Cells[5].Value.ToString();
                dateTimePicker1.Value = Convert.ToDateTime(selectedRow.Cells[1].Value.ToString());
                selectedShiftId = Convert.ToInt32(selectedRow.Cells[0].Value.ToString());
            }
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            var foundData = await _shiftService.GetByFilterAsync(x => x.Id == selectedShiftId);
            if (foundData != null)
            {
                var selectedMachine = await _machineService.GetByFilterAsync(x => x.MachineName == cmbMachines.SelectedValue);
                var selectedEmployee = await _employeeService.GetByFilterAsync(x => x.EmployeeName == cmbEmployees.SelectedValue);
                Shifts shift = foundData;
                shift.ShiftDate = dateTimePicker1.Value;
                shift.MachineId = selectedMachine.Id;
                shift.EmployeeId = selectedEmployee.Id;
                var shresult = await _shiftService.UpdateAsync(shift);
                if (shresult)
                {
                    dataGridView3.DataSource = await _shiftService.GetAllShiftsAsync();
                    MessageBox.Show("Shift verisi g�ncellendi.");
                }
            }
            else
                MessageBox.Show("Shift verisi bulunamad�.");
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            var foundData = await _shiftService.GetByFilterAsync(x => x.Id == selectedShiftId);
            if (foundData != null)
            {
                await _shiftService.RemoveAsync(foundData.Id);
                dataGridView3.DataSource = await _shiftService.GetAllShiftsAsync();
                MessageBox.Show("Shift verisi silindi.");
            }

        }

        private async void button8_Click(object sender, EventArgs e)
        {
            var foundData = await _machineService.GetByFilterAsync(x => x.Id == selectedMachineId);
            if (foundData != null)
            {
                await _machineService.RemoveAsync(foundData.Id);
                dataGridView2.DataSource = await _machineService.GetAllAsync();
                dataGridView3.DataSource = await _shiftService.GetAllShiftsAsync();
                MessageBox.Show("Makine verisi silindi.");
            }
        }

        private async void button9_Click(object sender, EventArgs e)
        {
            var foundData = await _employeeService.GetByFilterAsync(x => x.Id == selectedEmployeeId);
            if (foundData != null)
            {
                await _employeeService.RemoveAsync(foundData.Id);
                dataGridView1.DataSource = await _employeeService.GetAllAsync();
                dataGridView3.DataSource = await _shiftService.GetAllShiftsAsync();
                MessageBox.Show("Personel verisi silindi.");
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            string server = ServerName.Text;

            // Build the new connection string
            string newConnectionString = $"Server={server};Database=DijiTaskDB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;";

            // Update the connection string in ConfigurationManager
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.ConnectionStrings.ConnectionStrings["dijiTaskDb"].ConnectionString = newConnectionString;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");

            MessageBox.Show("Ba�lant� dizesi g�ncellendi.");
        }

        private string LoadConnectionStringParameters()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["dijiTaskDb"].ConnectionString;

            var builder = new System.Data.Common.DbConnectionStringBuilder();
            builder.ConnectionString = connectionString;

            if (builder.ContainsKey("Server"))
                ServerName.Text = builder["Server"].ToString();
            //if (builder.ContainsKey("Port"))
            //    PortNumber.Text = builder["Port"].ToString();
            //if (builder.ContainsKey("User Id"))
            //    UserName.Text = builder["User Id"].ToString();
            //if (builder.ContainsKey("Password"))
            //    Password.Text = builder["Password"].ToString();
            return connectionString;
        }

        private void Migration_Click(object sender, EventArgs e)
        {
            using (var dbContext = new DAL.AppDbContext())
            {
                bool isDatabaseExists = dbContext.Database.CanConnect();

                if (isDatabaseExists)
                {
                    MessageBox.Show("Veritaban� zaten mevcut.", "Uyar�", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (!isDatabaseExists)
                {
                    // Aray�zdeki ��eyi g�ncellemek i�in bir fonksiyon �a��r
                    UpdateStatusLabel("Veritaban� olu�turuluyor...");

                    // Veritaban�n� olu�tur veya g�ncelle
                    dbContext.Database.EnsureCreated();

                    // Aray�zdeki ��eyi g�ncellemek i�in bir fonksiyon �a��r
                    UpdateStatusLabel("Veritaban� olu�turma i�lemi ba�ar�yla tamamland�.");

                    // Timer'� ba�lat, 3 saniye sonra temizleme i�lemi yapacak
                    StartClearTimer();
                }


            }
        }

        // Aray�zdeki ��eyi g�ncellemek i�in kullan�lacak fonksiyon
        private void UpdateStatusLabel(string message)
        {
            // �rnek olarak, bir Label metni g�ncelleme
            lblStatus.Text = message;
            lblStatus.Update(); // G�ncellemenin hemen g�r�nt�lenmesi i�in
        }

        private void StartClearTimer()
        {
            _statusTimer = new System.Timers.Timer(3000); // 3000 ms (3 saniye) sonra tetiklenecek
            _statusTimer.Elapsed += ClearStatusLabel;
            _statusTimer.AutoReset = false; // Tek seferlik tetikleme
            _statusTimer.Start();
        }

        private void ClearStatusLabel(object sender, ElapsedEventArgs e)
        {
            // Timer taraf�ndan �a�r�ld���nda Label'� temizle
            if (InvokeRequired)
            {
                Invoke(new Action(() => lblStatus.Text = ""));
            }
            else
            {
                lblStatus.Text = "";
            }
        }
    }
}