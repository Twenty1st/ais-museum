using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Globalization;

namespace museumAIS.Classes
{
    public class createWordDoc
    {
        public void createWordDocument(System.Data.DataTable dataTable, string status)
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
                AddFormattedParagraph2(wordDoc, new string[] { "Отчет по Экспонатам: Музей Истории" }, 18,
                    WdParagraphAlignment.wdAlignParagraphCenter, new bool[] { true }, new bool[] { false });
                AddFormattedParagraph2(wordDoc, new string[] { "Дата отчета: " + formattedDate }, 14, 
                    WdParagraphAlignment.wdAlignParagraphLeft, new bool[] { true }, new bool[] { false });
                AddFormattedParagraph2(wordDoc, new string[] { "Раздел 1: Общая Информация" }, 14, 
                    WdParagraphAlignment.wdAlignParagraphLeft, new bool[] { true }, new bool[] { false });

                int rowIndex = 1;
                foreach (DataRow row in dataTable.Rows)
                {
                    string place = row["where_ecsp"].ToString();
                    if (place == "2")
                    {
                        place = "В резерве";
                    }
                    else if (!place.ToLower().Contains("зал"))
                    {
                        place = "Дата прибытия: " + place;
                    }

                    //AddFormattedParagraph2(wordDoc, new string[] { place }, 14, 
                    //    WdParagraphAlignment.wdAlignParagraphLeft, new bool[] { true }, new bool[] { false });
                    string text = rowIndex + ". " + row["name_ecsponat"].ToString();
                    AddFormattedParagraph2(wordDoc, new string[] { ": " + place,  text}, 14, 
                        WdParagraphAlignment.wdAlignParagraphLeft, new bool[] { false, true }, new bool[] { false, false });
                    
                    AddFormattedParagraph2(wordDoc, new string[] {row["date_create_ecsponat"].ToString(), "Дата создания: "}, 
                        14, WdParagraphAlignment.wdAlignParagraphLeft, new bool[] { false, true }, new bool[] { false, false });
                    
                    AddFormattedParagraph2(wordDoc, new string[] { row["description_ecsponat"].ToString(), "Описание: " }, 
                        14, WdParagraphAlignment.wdAlignParagraphLeft, new bool[] { false, true }, new bool[] { false, false });

                    rowIndex++;
                }

                AddFormattedParagraph2(wordDoc, new string[] { "Раздел 2: Подробный Список Экспонатов" }, 14, 
                    WdParagraphAlignment.wdAlignParagraphLeft, new bool[] { true }, new bool[] { false });


                // Добавляем заголовки и данные
                Paragraph para = wordDoc.Content.Paragraphs.Add();
                // Создаем таблицу
                Table table = wordDoc.Tables.Add(para.Range, dataTable.Rows.Count + 1, 6);
                table.Range.Font.Name = "Times New Roman";
                table.Range.Font.Size = 14;

                // Заполняем заголовки столбцов
                table.Cell(1, 1).Range.Text = "№";
                table.Cell(1, 2).Range.Text = "Наименование";
                table.Cell(1, 3).Range.Text = "Описание";
                table.Cell(1, 4).Range.Text = "Дата создания";
                table.Cell(1, 5).Range.Text = "Место расположения";
                table.Cell(1, 6).Range.Text = "Картинка";

                // Заполняем данные из DataTable
                rowIndex = 2;
                foreach (DataRow row in dataTable.Rows)
                {
                    string place = row["where_ecsp"].ToString();
                    if (place == "2")
                    {
                        place = "В резерве";
                    }else if (!place.ToLower().Contains("зал"))
                    {
                        place = "Дата прибытия: " + place;
                    }


                    table.Cell(rowIndex, 1).Range.Text = (rowIndex - 1).ToString();
                    table.Cell(rowIndex, 2).Range.Text = row["name_ecsponat"].ToString(); ;
                    table.Cell(rowIndex, 3).Range.Text = row["description_ecsponat"].ToString();
                    table.Cell(rowIndex, 4).Range.Text = row["date_create_ecsponat"].ToString();
                    table.Cell(rowIndex, 5).Range.Text = place;
                    //table.Cell(rowIndex, 6).Range.Text = row["image_ecsponat"].ToString();
                    // Вставляем изображение по пути, указанному в ячейке
                    string imagePath = Directory.GetCurrentDirectory() + "\\images\\"+row["image_ecsponat"].ToString();
                    if (System.IO.File.Exists(imagePath))
                    {
                        Range cellRange = table.Cell(rowIndex, 6).Range;
                        cellRange.InlineShapes.AddPicture(imagePath, LinkToFile: false, SaveWithDocument: true);
                    }
                    else
                    {
                        table.Cell(rowIndex, 6).Range.Text = "Изображение не найдено";
                    }
                    rowIndex++;
                }

                string savePath = chooseDirectories.dirDocuments + "\\Очет"+DateTime.Today.ToString("d MMM")+"экспонаты "+ status +".docx";
                // Сохраняем новый документ
                wordDoc.SaveAs(savePath);
                // Открываем новый документ
                wordApp.Visible = true;
                // Показываем документ пользователю
                wordApp.Visible = true;
            }
            catch (Exception ex)
            {
                wordDoc.Close(false);
                wordApp.Quit();
                throw ex;
            }
            finally
            {
                // Освобождаем ресурсы
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordDoc);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
            }
        }
        private void AddFormattedParagraph(Document doc, string text, int fontSize, WdParagraphAlignment alignment)
        {
            Paragraph para = doc.Content.Paragraphs.Add();
            para.Range.Text = text;
            para.Range.Font.Name = "Times New Roman";
            para.Range.Font.Size = fontSize;
            para.Alignment = alignment;
            para.Range.InsertParagraphAfter();
        }

        private void AddFormattedParagraph2(Document doc, string[] parts, int fontSize, WdParagraphAlignment alignment, bool[] isBold, bool[] isItalic)
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
