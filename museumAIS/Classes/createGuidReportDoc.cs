using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace museumAIS.Classes
{
    public class createGuidReportDoc
    {
        public static void createWordDocument(System.Data.DataTable dataTable)
        {
            // Создаем новое приложение Word
            Application wordApp = new Application();
            Document wordDoc = wordApp.Documents.Add();

            try
            {
                // Устанавливаем поля документа
                wordDoc.PageSetup.TopMargin = wordApp.CentimetersToPoints(1);
                wordDoc.PageSetup.BottomMargin = wordApp.CentimetersToPoints(2);
                wordDoc.PageSetup.LeftMargin = wordApp.CentimetersToPoints(1);
                wordDoc.PageSetup.RightMargin = wordApp.CentimetersToPoints(1);

                DateTime date = DateTime.Now; // Используйте свою дату здесь
                CultureInfo cultureInfo = new CultureInfo("ru-RU");
                var formattedDate = date.ToString("dd MMMM yyyy 'года'", cultureInfo);


                // Добавляем заголовки и данные с форматированием
                AddFormattedParagraph2(wordDoc, new string[] { "Музей Истории" }, 18,
                    WdParagraphAlignment.wdAlignParagraphCenter, new bool[] { true }, new bool[] { false }); 
                AddFormattedParagraph2(wordDoc, new string[] { "Табель Учета Рабочего Времени Экскурсоводов" }, 14,
                    WdParagraphAlignment.wdAlignParagraphLeft, new bool[] { true }, new bool[] { false });
                AddFormattedParagraph2(wordDoc, new string[] { formattedDate }, 14,
                    WdParagraphAlignment.wdAlignParagraphLeft, new bool[] { true }, new bool[] { false });

                // Добавляем заголовки и данные
                Paragraph para = wordDoc.Content.Paragraphs.Add();
                // Создаем таблицу
                Table table = wordDoc.Tables.Add(para.Range, dataTable.Rows.Count + 1, 3);
                table.Range.Font.Name = "Times New Roman";
                table.Range.Font.Size = 14;

                // Установить ширину первого столбца
                table.Columns[1].Width = 50; // Поиграйте с этим числом, чтобы найти нужную ширину

                // Установить ширину таблицы
                table.AutoFitBehavior(Microsoft.Office.Interop.Word.WdAutoFitBehavior.wdAutoFitWindow);

                // Заполняем заголовки столбцов
                table.Cell(1, 1).Range.Text = "№";
                table.Cell(1, 2).Range.Text = "ФИО Экскурсовода";
                table.Cell(1, 3).Range.Text = "Рабочие Часы в Месяце";

                // Заполняем данные из DataTable
                int rowIndex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    string fio = row["guid_lastName"].ToString().Split(' ')[0] + " " + row["guid_name"].ToString() + " " + row["guid_patronamyc"].ToString();

                    table.Cell(rowIndex, 1).Range.Text = (rowIndex - 1).ToString();
                    table.Cell(rowIndex, 2).Range.Text = fio;
                    table.Cell(rowIndex, 3).Range.Text = row["guidWorkTime"].ToString();

                    rowIndex++;
                }


                AddFormattedParagraph2(wordDoc, new string[] { "Подписи:" }, 14,
                    WdParagraphAlignment.wdAlignParagraphLeft, new bool[] { true }, new bool[] { false });

                AddFormattedParagraph(wordDoc, "Ответственный за учет рабочего времени: ___________ (подпись)", 14,  WdParagraphAlignment.wdAlignParagraphLeft);
                AddFormattedParagraph(wordDoc, "Дата: ___________", 14,  WdParagraphAlignment.wdAlignParagraphLeft);


                // Показываем документ пользователю
                wordApp.Visible = true;
            }
            catch (Exception ex)
            {
                wordDoc.Close(false);
                wordApp.Quit();

            }
            finally
            {
                // Освобождаем ресурсы
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordDoc);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
            }
        }
        private static void AddFormattedParagraph(Document doc, string text, int fontSize, WdParagraphAlignment alignment)
        {
            Paragraph para = doc.Content.Paragraphs.Add();
            para.Range.Text = text;
            para.Range.Font.Name = "Times New Roman";
            para.Range.Font.Size = fontSize;
            para.Alignment = alignment;
            para.Range.InsertParagraphAfter();
        }

        private static void AddFormattedParagraph2(Document doc, string[] parts, int fontSize, WdParagraphAlignment alignment, bool[] isBold, bool[] isItalic)
        {
            if (parts.Length != isBold.Length || parts.Length != isItalic.Length)
            {
                throw new ArgumentException("The text parts, bold flags and italic flags must have the same length");
            }

            Paragraph para = doc.Content.Paragraphs.Add();
            para.Range.Font.Name = "Times New Roman";
            para.Range.Font.Size = fontSize;
            para.Alignment = alignment;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                Range range = para.Range;

                // Разделите текущий диапазон на части
                int start = range.Start + range.Text.Length;
                range.InsertAfter(part);
                Range partRange = doc.Range(start, start + part.Length);

                // Применить форматирование
                if (isBold[i])
                {
                    partRange.Font.Bold = 1;
                }
                else
                {
                    partRange.Font.Bold = 0;
                }

                if (isItalic[i])
                {
                    partRange.Font.Italic = 1;
                }
                else
                {
                    partRange.Font.Italic = 0;
                }
            }
            //para.Range.InsertParagraphAfter();
        }
    }
}
