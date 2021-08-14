﻿using System;
using System.Timers;
using System.IO;
using System.Collections;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace Typing_App
{
    interface UploadToJson
    {
        void uploadtojson();
    }
    interface showResult
    {
        void ShowResult();
    }

    public abstract class Grade : UploadToJson, showResult
    {
        private int goodWords;
        private ArrayList badWords = new ArrayList();
        private int chars;
        private int backTime;
        private int testTime;

        public Grade(int _goodWords, ArrayList _badWords, int _chars, int _backTime, int _testTime)
        {
            goodWords = _goodWords;
            badWords = _badWords;
            chars = _chars;
            backTime = _backTime;
            testTime = _testTime;
        }
        public int GoodWords { get { return goodWords; } set { goodWords = value; } }
        public ArrayList BadWords { get { return badWords; } set { badWords = value; } }
        public int Chars { get { return chars; } set { chars = value; } }
        public int BackTime { get { return backTime; } set { backTime = value; } }
        public int TestTime { get { return testTime; } set { testTime = value; } }

        public double Accuracy()
        {
            return Convert.ToDouble(GoodWords) /
                Convert.ToDouble(GoodWords + BadWords.Count);
        }
        public double Backspace_Freq()
        {
            return Convert.ToDouble(BackTime) /
                Convert.ToDouble(Chars);
        }
        public virtual int wpm()
        {
            return 0;
        }
        public int cpm()
        {
            return Chars / TestTime;
        }
        public void ShowResult()
        {
            Console.WriteLine(
                "WPM: " + wpm() + ",  CPM: " + cpm() +
                ",  Accuracy: " + Convert.ToDouble(Convert.ToInt32(Accuracy() * 1000)) / 10 + "%" +
                ",  BackSpace Frequency: " + Convert.ToDouble(Convert.ToInt32(Backspace_Freq() * 1000)) / 10 + "%");
        }
        public void uploadtojson()
        {

            var json_element = new Json_element
            {
                DateTime = DateTime.Now.ToString()
                    .Replace("\u4E0B\u5348", "P.M.").Replace("\u4E0A\u5348", "A.M."),
                TestLength = TestTime,
                WPM = wpm(),
                CPM = cpm(),
                Accuracy = Accuracy(),
                BackRate = Backspace_Freq(),
                WrongWords = (String[])BadWords.ToArray(typeof(string))
            };
            string json = System.Text.Json.JsonSerializer.Serialize(json_element);
            File.AppendAllText(@"TypingPracticeRecords.json", "," + json + "]");
        }
    }
    public class Json_element
    {
        public string DateTime { get; set; }
        public int TestLength { get; set; }
        public int WPM { get; set; }
        public int CPM { get; set; }
        public double Accuracy { get; set; }
        public double BackRate { get; set; }
        public string[] WrongWords { get; set; }
    }
    public class Common_Calculate_Way : Grade
    {
        public Common_Calculate_Way(int _goodWords, ArrayList _badWords, int _chars, int _backTime, int _testTime)
                                                       : base(_goodWords, _badWords, _chars, _backTime, _testTime)
        {
            TestTime = _testTime;
            GoodWords = _goodWords;
            BadWords = _badWords;
            Chars = _chars;
            BackTime = _backTime;
        }
        public override int wpm()
        {
            return cpm() / 5;
        }
    }
    public class For_Long_Time_Test : Grade
    {
        public For_Long_Time_Test(int _goodWords, ArrayList _badWords, int _chars, int _backTime, int _testTime)
                                                     : base(_goodWords, _badWords, _chars, _backTime, _testTime)
        {
            TestTime = _testTime;
            GoodWords = _goodWords;
            BadWords = _badWords;
            Chars = _chars;
            BackTime = _backTime;
        }
        public override int wpm()
        {
            return GoodWords / TestTime;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ushort TestTime = 1;
            void Exception()
            {
                try
                {
                    Console.WriteLine("How long do you want this test be ?\n");
                    Console.Write("Please Enter (minute): (Ex: 3)");
                    TestTime = Convert.ToUInt16(Console.ReadLine());
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine("\n" + e.Message + "系統將自動判定為 1");
                }
                catch (OutOfMemoryException e)
                {
                    Console.Error.WriteLine("\n" + e.Message + "系統將自動判定為 1");
                }
                catch (ArgumentException e)
                {
                    Console.Error.WriteLine("\n" + e.Message + "系統將自動判定為 1");
                }
                catch (FormatException e)
                {
                    Console.Error.WriteLine("\n" + e.Message + "系統將自動判定為 1");
                }
                catch (System.OverflowException e)
                {
                    Console.Error.WriteLine("\n" + e.Message + "系統將自動判定為 1");
                }
                finally
                {
                    if (TestTime == 0)
                    {
                        Console.Error.WriteLine("不可為 0");
                        Exception();
                    }
                    else
                    {
                        Console.WriteLine("\nThis test is going to be " + TestTime +
                            " minites long. Press Enter to continue.");
                        Console.ReadLine();
                        Console.WriteLine("Ok, Let's Go !");
                    }
                }
            }
            StreamReader _preWord = new StreamReader(@"TypingPracticeRecords.json");
            string AllJsonContent = _preWord.ReadToEnd();
            _preWord.Close();
            StreamWriter preWork = new StreamWriter(@"TypingPracticeRecords.json");
            preWork.Write(AllJsonContent.Remove(AllJsonContent.Length - 2));
            preWork.Close();
            Exception();

            For_Long_Time_Test _g = new For_Long_Time_Test(0, new ArrayList(), 0, 0, TestTime);
            Common_Calculate_Way g = new Common_Calculate_Way(0, new ArrayList(), 0, 0, TestTime);

            Timer timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(Timesup);
            timer.AutoReset = true;
            timer.Interval = TestTime * 1000 * 60;
            timer.Enabled = true;

            void Timesup(object source, ElapsedEventArgs e)
            {
                Console.WriteLine();
                Console.WriteLine("-------------------------------------------" +
                    "------------------------------------------------------------------");
                Console.WriteLine(".");
                Console.WriteLine(".");
                Console.WriteLine(".");
                g.ShowResult();
                g.uploadtojson();
                timer.Close();
                Console.Read();
            }

            ArrayList recordRandom = new ArrayList();
            ArrayList word = new ArrayList();

            int state = 1;

            StreamReader jsonfile = new StreamReader(@"Database.json");
            string data = jsonfile.ReadToEnd();

            JObject Dictionary = JObject.Parse(JArray.Parse(data)[0].ToString());

            JArray arrs = (JArray)Dictionary["Words List"];

            string[] arr = arrs.ToObject<string[]>();

            DateTime now = DateTime.Now;
            string Now = now.ToString();
            int secondNow = now.Second;
            int minuteNow = now.Minute;
            int hourNow = now.Hour;
            int yearNow = now.Year;
            int randomSeed = secondNow + minuteNow + hourNow + yearNow;

            createLine();
            input();

            void createLine()
            {
                recordRandom.Clear();
                recordRandom.Add("");
                Console.WriteLine();
                int i = 10;
                Random r = new Random(i * state + randomSeed + TestTime);
                int n = r.Next(0, arr.Length - 1000);
                while (i != 0)
                {
                    Console.Write("\u001b[1m" + arr[n] + "\u001b[0m      ");
                    recordRandom.Add(arr[n]);
                    n += 1;
                    i--;
                }
                Console.WriteLine();
            }

            void input()
            {
                ConsoleKeyInfo k = Console.ReadKey();
                if (k.KeyChar == ' ')
                {
                    string result = "";
                    foreach (char element in word) result = result + Convert.ToString(element);
                    if (state % 10 == 0)
                    {
                        if (result == Convert.ToString(recordRandom[10]))
                        {
                            Console.Write(" \u001b[38;5;82m(O)\u001b[0m ");
                            g.GoodWords++;
                        }
                        else
                        {
                            Console.Write(" \u001b[38;5;160m(X)\u001b[0m ");
                            g.BadWords.Add(Convert.ToString(recordRandom[10]));
                        }
                        Console.WriteLine();
                        createLine();
                    }
                    else
                    {
                        if (result == Convert.ToString(recordRandom[state % 10]))
                        {
                            Console.Write(" \u001b[38;5;82m(O)\u001b[0m ");
                            g.GoodWords++;
                        }
                        else
                        {
                            Console.Write(" \u001b[38;5;160m(X)\u001b[0m ");
                            g.BadWords.Add(Convert.ToString(recordRandom[state % 10]));
                        }
                    }
                    word.Clear();
                    state++;
                    input();
                }
                else if (k.Key == ConsoleKey.Backspace)
                {
                    if (word.Count - 1 >= 0)
                    {
                        word.RemoveAt(word.Count - 1);
                        g.BackTime++;
                        g.Chars--;
                    }
                    input();
                }
                else
                {
                    word.Add(k.KeyChar);
                    g.Chars++;
                    input();
                }
            }
        }
    }
}
