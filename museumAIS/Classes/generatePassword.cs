using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace museumAIS
{
    public class generatePassword
    {
        public StringBuilder getRandmPwd()
        {
            // Создаем объект Random для генерации случайных чисел
            Random random = new Random();

            // Генерируем массив из символов, включающий буквы верхнего и нижнего регистра, цифры и знаки препинания
            char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^& * ()".ToCharArray();

            // Создаем пустую строку для хранения пароля
            StringBuilder password = new StringBuilder();

            // Заполняем строку 8 случайными символами
            for (int i = 0; i < 8; i++)
            {
                // Получаем случайный индекс символа в массиве
                int index = random.Next(chars.Length);

                // Добавляем символ в строку
                password.Append(chars[index]);
            }

            return password;
        }
        
    }
}
