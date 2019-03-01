﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MNN
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void MainThread()
        {
            //for (;;) Console.WriteLine("Ciao");

            /*
            double[] input = new double[785];
            double[] y = new double[10];
            
            for(int i = 1; i <= 784; i++)
            {
                input[i] = 1;
            }

            y[0] = 0.5;
            y[1] = 0.3;
            y[2] = 0.8;
            y[3] = 0.1;
            y[4] = 0;
            y[5] = 1;
            y[6] = 0.7;
            y[7] = 0.5;
            y[8] = 0.6;
            y[9] = 1;

            Example e = new Example(input, y);
            NeuralNetwork NN = new NeuralNetwork();
            
            NN.ForwardPropagate(e);
            Console.WriteLine("Foward Propagated");
            NN.Output();

            Example[] e2 = new Example[1];
            e2[0] = e;

            for(; ; )
            {
                NN.BackPropagate(e2);
                NN.ForwardPropagate(e);
                NN.Output();
                Console.WriteLine("           ");
            }
            */
            string[] Args;
            string R, temp;

            NeuralNetwork NN = new NeuralNetwork();


            do
            {
                Console.Write("MNN>>");
                temp = Console.ReadLine();
                temp = temp.ToLower();
                Args = temp.Split(null);
                
                switch (Args[0])
                {
                    case "gui":
                        
                        break;
                    
                    case "network":
                        if (Args.Length == 1)
                        {
                            Console.WriteLine("ciao");
                        }
                        else if (Args[1] == "load") {
                            NN.LoadFile(Args[2]);
                            /*foreach(int i in NN.LoadFile(Args[2]))
                            {
                                Console.WriteLine(i);
                            }*/
                        }
                        else if (Args[1] == "init")
                        {
                            NN.Initialise();
                        }
                        else if (Args[1] == "save")
                        {
                            NN.WriteFile(Args[2]);
                        }
                        else if (Args[1] == "learn")
                        {
                            int n = Convert.ToInt32(Args[2]);
                            NN.MachineLearning(n);

                        }
                        else if (Args[1] == "run")
                        {
                            NN.Run(Args[2]);
                        }
                        break;

                    case "trainingset":
                        if (Args.Length == 1)
                        {
                            Console.WriteLine("ciao");
                        }
                        else if (Args[1] == "load")
                        {
                            NN.LoadTS(Args[2]);
                        }
                        else if (Args[1] == "extract")
                        {
                            NN.ExtractTS(Int32.Parse(Args[2]));
                        }
                        break;

                    case "test":
                        
                            int i = 0;
                        double[][,] g = new double[3][,];
                        g[0] = new double[16, 785];
                        g[1] = new double[16, 17];
                        g[2] = new double[10, 17];
                        foreach (double[,] a in g)
                        {
                            foreach (double b in a)
                            {
                                Console.WriteLine(a.GetLength(0));
                                Console.WriteLine(a.GetLength(1));
                            }
                        }

                        break;
                    case "stop":
                        break;
                    default:
                        R = "Comando non riconosciuto. Usare il comando 'help' per mostrare l'elenco dei comandi";
                        Console.WriteLine(R);
                        break;
                }
                

            }
            while (true);

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new Thread(new ThreadStart(MainThread)).Start();
        }
    }
}
