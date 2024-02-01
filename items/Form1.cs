using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace items
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            FillDataGridView();
            itemsDataTable = new DataTable();
            // Заполнение combox
            PopulateComboBox();
        }
        public string connectionString = "Server=localhost;Port=5432;Database=database;Username=postgres;Password=0000;";
        int selectedRow;
        private int currentPage = 1;
        private int itemsPerPage = 5;
        private DataTable itemsDataTable;


        private void FillDataGridView()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                int offset = (currentPage - 1) * itemsPerPage;

                string query = $"SELECT * FROM items ORDER BY id_товаров OFFSET {offset} ROWS FETCH NEXT {itemsPerPage} ROWS ONLY";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        dataGridView1.DataSource = table;
                    }
                }
            }
        }


        private void button_look(object sender, EventArgs e)
        {
            string itemName = textBox1.Text.Trim();
            string query = $"SELECT * FROM items WHERE наименование_продукта ILIKE '%{itemName}%'";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        dataGridView1.DataSource = table;
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == "По убыванию")
            {
                dataGridView1.Sort(dataGridView1.Columns[3], ListSortDirection.Descending);
            }
            if (comboBox1.SelectedItem == "По возрастанию")
            {
                dataGridView1.Sort(dataGridView1.Columns[3], ListSortDirection.Ascending);
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox1.Text.Trim();
            SearchAndDisplayResults(searchText);
        }
        private void SearchAndDisplayResults(string searchText)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM items WHERE наименование_продукта ILIKE '%{searchText}%'";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        dataGridView1.DataSource = table;
                    }
                }
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("По убыванию");
            comboBox1.Items.Add("По возрастанию");

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }
        private void DrawBarcode(string code, int resolution = 20) // resolution - пикселей на миллиметр
        {
            int numberCount = 15; // количество цифр
            float height = 25.93f * resolution; // высота штрих кода
            float lineHeight = 22.85f * resolution; // высота штриха
            float leftOffset = 3.63f * resolution; // свободная зона слева
            float rightOffset = 2.31f * resolution; // свободная зона справа
                                                    //штрихи, которые образуют правый и левый ограничивающие знаки,
                                                    //а также центральный ограничивающий знак должны быть удлинены вниз на 1,65мм
            float longLineHeight = lineHeight + 1.65f * resolution;
            float fontHeight = 2.75f * resolution; // высота цифр
            float lineToFontOffset = 0.165f * resolution; // минимальный размер от верхнего края цифр до нижнего края штрихов
            float lineWidthDelta = 0.15f * resolution; // ширина 0.15*{цифра}
            float lineWidthFull = 1.35f * resolution; // ширина белой полоски при 0 или 0.15*9
            float lineOffset = 0.2f * resolution; // между штрихами должно быть расстояние в 0.2мм

            float width = leftOffset + rightOffset + 6 * (lineWidthDelta + lineOffset) + numberCount * (lineWidthFull + lineOffset); // ширина штрих-кода

            Bitmap bitmap = new Bitmap((int)width, (int)height); // создание картинки нужных размеров
            Graphics g = Graphics.FromImage(bitmap); // создание графики

            Font font = new Font("Arial", fontHeight, FontStyle.Regular, GraphicsUnit.Pixel); // создание шрифта

            StringFormat fontFormat = new StringFormat(); // Центрирование текста
            fontFormat.Alignment = StringAlignment.Center;
            fontFormat.LineAlignment = StringAlignment.Center;

            float x = leftOffset; // позиция рисования по x
            for (int i = 0; i < numberCount; i++)
            {
                int number = Convert.ToInt32(code[i].ToString()); // число из кода
                if (number != 0)
                {
                    g.FillRectangle(Brushes.Black, x, 0, number * lineWidthDelta, lineHeight); // рисуем штрих
                }
                RectangleF fontRect = new RectangleF(x, lineHeight + lineToFontOffset, lineWidthFull, fontHeight); // рамки для буквы
                g.DrawString(code[i].ToString(), font, Brushes.Black, fontRect, fontFormat); // рисуем букву
                x += lineWidthFull + lineOffset; // смещаем позицию рисования по x

                if (i == 0 && i == numberCount / 2 && i == numberCount - 1) // если это начало, середина или конец кода рисуем разделители
                {
                    for (int j = 0; j < 2; j++) // рисуем 2 линии разделителя
                    {
                        g.FillRectangle(Brushes.Black, x, 0, lineWidthDelta, longLineHeight); // рисуем длинный штрих
                        x += lineWidthDelta + lineOffset; // смещаем позицию рисования по x
                    }
                }
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom; // делаем чтобы картинка помещалась в pictureBox
                pictureBox1.Image = bitmap; // устанавливаем картинку
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            DrawBarcode(textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                FillDataGridView();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string countQuery = "SELECT COUNT(*) FROM items";
                using (NpgsqlCommand countCmd = new NpgsqlCommand(countQuery, connection))
                {
                    int totalItems = Convert.ToInt32(countCmd.ExecuteScalar());
                    int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

                    if (currentPage < totalPages)
                    {
                        currentPage++;
                        FillDataGridView();
                    }
                }
            }
        }


        private void dataGridView1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedRow = e.RowIndex;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[selectedRow];

                textBox1.Text = row.Cells[1].Value.ToString();

            }
        }
        private void PopulateComboBox()
        {
            // Заполнение combox у
            var categories = itemsDataTable.AsEnumerable().Select(row => row.Field<string>("category")).Distinct().ToList();
            comboBox2.Items.AddRange(categories.ToArray());
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCategory = comboBox2.SelectedItem.ToString();

            // Выполняем запрос в зависимости от выбранной категории или выводим все товары
            string query = (selectedCategory == "все")
                ? "select * from items"
                : $"select * from items where категория LIKE '{selectedCategory}'";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
                    {
                        // Очищаем DataTable перед заполнением
                        itemsDataTable.Clear();

                        // Заполняем DataTable
                        adapter.Fill(itemsDataTable);

                        // Обновляем DataGridView
                        dataGridView1.DataSource = itemsDataTable;
                    }
                }
            }
        }
    }
}



