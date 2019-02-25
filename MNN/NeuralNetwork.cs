using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNN
{
    public struct Example
    {
        public double[] x;
        public double[] y;

        public Example(double[] a,double[] b)
        {
            x = new double[784];
            for (int i = 0; i < 784; i++) x[i] = a[i];
            y = new double[10];
            for (int i = 0; i < 10; i++) y[i] = b[i];
        }
    }

    class NeuralNetwork
    {
        const double _learningFactor = 0.2;
        const double _momentum = 0.2;

        double[][] layer = new double[4][];
        double[][,] weight = new double[3][,];
        bool loaded, tsLoaded;
        List<Example> examples = new List<Example>();
        int exNo;
        //double[] y = new double[10];


        public NeuralNetwork()
        {
            layer[0] = new double[785];
            layer[1] = new double[17];
            layer[2] = new double[17];
            layer[3] = new double[10];
            layer[0][0] = 1;
            layer[1][0] = 1;
            layer[2][0] = 1;
            weight[0] = new double[17, 785];
            weight[1] = new double[17, 17];
            weight[2] = new double[10, 17];
            exNo = 0;

            loaded = false;
            tsLoaded = false;
        }

        /*public IEnumerable<int> LoadFile(string path)
        {
            using (BinaryReader br = new BinaryReader(new FileStream(path, FileMode.Open)))
            {

                for (int i = 0; i < 6; i++)
                {
                    int x = BitConverter.ToInt32(br.ReadBytes(sizeof(Int32)), 0);
                    yield return x;

                }
                
            }
            loaded = true;
        }*/
        
        public void Initialise()
        {
            Random rand = new Random();
            double u1, u2, randStdNormal;
            
            for (int i=0;i<17;i++) {
                for (int j = 0; j < 785; j++)
                {
                    u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
                    u2 = 1.0 - rand.NextDouble();
                    //Console.WriteLine(u1);
                    //Console.WriteLine(u2);
                    randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                    //Console.WriteLine(randStdNormal);
                    weight[0][i, j] = randStdNormal * Math.Sqrt((double)2 / (17 + 785));
                    //Console.WriteLine(weight[0][i, j]);
                }
            }

            for (int i = 0; i < 17; i++)
            {
                for (int j = 0; j < 17; j++)
                {
                    u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
                    u2 = 1.0 - rand.NextDouble();
                    randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                    weight[1][i, j] = randStdNormal * Math.Sqrt((double)2 / (17 + 17));
                }
            }

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 17; j++)
                {
                    u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
                    u2 = 1.0 - rand.NextDouble();
                    randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                    weight[2][i, j] = randStdNormal * Math.Sqrt((double)2 / (10 + 17));
                }
            }

    
    
    /*
            for(int i = 0; i < 17; i++) //INIZIALIZZAZIONE RANDOM TEMPORANEA
            {
                for (int j = 0; j < 785; j++)
                {
                    weight[0][i, j] = rand.NextDouble();
                }
            }
            for (int i = 0; i < 17; i++)
            {
                for (int j = 0; j < 17; j++)
                {
                    weight[1][i, j] = rand.NextDouble();
                }
            }
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 17; j++)
                {
                    weight[2][i, j] = rand.NextDouble();
                }
            }*/
            Console.WriteLine("Rete Neurale Inizializzata");
            loaded = true;
        }

        public void LoadFile(string path)
        {
            int t_weightN;
            int[] t_layer;
            double[][,] t_weight;
            using (BinaryReader br = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                t_weightN=BitConverter.ToInt32(br.ReadBytes(sizeof(Int32)), 0)-1;

                t_weight = new double[t_weightN][,];
                t_layer = new int[t_weightN + 1]; //Il numero di layer è uguale al numeri di classi di pesi aumentato di 1

                for(int i = 0; i <= t_weightN; i++)
                {
                    t_layer[i] = BitConverter.ToInt32(br.ReadBytes(sizeof(Int32)), 0);
                }

                for (int i = 0; i < t_weightN; i++)
                {
                    t_weight[i] = new double[t_layer[i+1],t_layer[i]];
                }

                //Struttura t_weight parallela a weight creata

                for(int N = 0; N < t_weightN; N++)
                {
                    for(int i = 0; i < t_layer[N + 1]; i++)
                    {
                        for(int j = 0; j < t_layer[N]; j++)
                        {
                            t_weight[N][i, j] = BitConverter.ToDouble(br.ReadBytes(sizeof(Double)), 0);
                        }
                    }
                }
                //Copia t_weight in weight
                for (int N = 0; N < t_weightN; N++)
                {
                    for (int i = 0; i < t_layer[N + 1]; i++)
                    {
                        for (int j = 0; j < t_layer[N]; j++)
                        {
                            weight[N][i, j] = t_weight[N][i, j];
                        }
                    }
                }

                exNo = BitConverter.ToInt32(br.ReadBytes(sizeof(Int32)), 0);

            }
            loaded = true;
        }
        //TODO MachineLearning() che propaga avanti e indietro il segnale con batch da 10 esempi presi dal trainingset. Controlla andamento costfunction
        public void MachineLearning(int N)
        {
            Console.WriteLine(loaded);
            Console.WriteLine(tsLoaded);
            if (loaded == false || tsLoaded == false) return;
            List<Example> temp = new List<Example>();
           
            for (int i = 0; i < N; i++)
            {
                
                temp = examples.GetRange(exNo, 10);
                exNo+=10;
                BackPropagate(temp.ToArray());
                Console.WriteLine("Costo iterazione " + i + " sulla batch "+exNo/10+":");
                
            }
        }

        public void WriteFile(string path)
        {
            using (BinaryWriter bw = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate)))
            {

                bw.Write((Int32)4);         //NUMERO DI LAYER
                bw.Write((Int32)785);       //LAYER 0
                bw.Write((Int32)17);        //LAYER 1
                bw.Write((Int32)17);        //LAYER 2
                bw.Write((Int32)10);        //LAYER 3
                

                for(int i = 0; i < 17; i++)
                {
                    for(int j = 0; j < 785; j++)
                    {
                        bw.Write((double)weight[0][i,j]);
                    }
                }

                for (int i = 0; i < 17; i++)
                {
                    for (int j = 0; j < 17; j++)
                    {
                        bw.Write((double)weight[1][i, j]);
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 17; j++)
                    {
                        bw.Write((double)weight[2][i, j]);
                    }
                }

                bw.Write((Int32)exNo);
            }
        }

        public void LoadTS(string path) {
            double[] x = new double[784];
            double[] y = new double[10];
            int rows, col, N, magic_img, magic_lab, temp;
            Console.WriteLine(path + "\\train-images.idx3-ubyte");
            Console.WriteLine(path + "\\train-labels.idx1-ubyte");
            using (BinaryReader br_img = new BinaryReader(new FileStream(path+"\\train-images.idx3-ubyte", FileMode.Open)))
            using (BinaryReader br_lab = new BinaryReader(new FileStream(path+"\\train-labels.idx1-ubyte", FileMode.Open)))
            {
                magic_img = BitConverter.ToInt32(br_img.ReadBytes(sizeof(Int32)).Reverse().ToArray(), 0); //L'istruzione Reverse() serve perchè il PC è low endian
                N = BitConverter.ToInt32(br_img.ReadBytes(sizeof(Int32)).Reverse().ToArray(), 0);
                rows = BitConverter.ToInt32(br_img.ReadBytes(sizeof(Int32)).Reverse().ToArray(), 0);
                col = BitConverter.ToInt32(br_img.ReadBytes(sizeof(Int32)).Reverse().ToArray(), 0);
                Console.WriteLine("Magic number Img: "+magic_img);
                Console.WriteLine("N: " + N);
                Console.WriteLine("rows:" + rows);
                Console.WriteLine("col:" + col);
                magic_lab = BitConverter.ToInt32(br_lab.ReadBytes(sizeof(Int32)).Reverse().ToArray(), 0);
                N = BitConverter.ToInt32(br_lab.ReadBytes(sizeof(Int32)).Reverse().ToArray(), 0); //RIDONDANZA

                for(int i = 0; i < N; i++)
                {
                    for(int j = 0; j < col * rows; j++)
                    {
                        x[j] = br_img.ReadByte();

                        //Console.Write(x[j]);
                        //Console.Write("\t");
                        //Console.WriteLine(x[j]);
                        //if (j % 28 == 0) Console.Write("\n");
                    }
                    temp = (int)br_lab.ReadByte();
                    for (int j = 0; j < 10; j++)
                    {
                        if (temp == j) y[j] = 1.0;
                        else y[j] = 0.0;
                    }
                    examples.Add(new Example(x, y));
                    //Console.WriteLine((int)br_lab.ReadByte());
                    //Console.Write("\n");
                }
            }
            tsLoaded = true;
        }

        public void ForwardPropagate(Example e)
        {
            double s = 0;

            //INPUT LAYER
            for(int i = 1; i <= 784; i++)
            {
                layer[0][i] = e.x[i-1];
            }


            //HIDDEN LAYER 1
            for(int j = 1; j <= 16; j++)
            {
                s = 0;
                for(int i = 0; i < 785; i++)
                {
                    s += weight[0][j, i] * layer[0][i];
                }
                layer[1][j] = Sigmoid(s);
            }


            //HIDDEN LAYER 2
            for (int j = 1; j <= 16; j++)
            {
                s = 0;
                for (int i = 0; i < 17; i++)
                {
                    s += weight[1][j, i] * layer[1][i];
                }
                layer[2][j] = Sigmoid(s);
            }


            //OUTPUT LAYER
            for (int j = 0; j < 10; j++)
            {
                s = 0;
                for (int i = 0; i < 17; i++)
                {
                    s += weight[2][j, i] * layer[2][i];
                }
                layer[3][j] = Sigmoid(s);
            }
        }

        public void BackPropagate(Example[] e)
        {
            double[][,] gradient = new double[3][,];
            gradient[0] = new double[16, 785];
            gradient[1] = new double[16, 17];
            gradient[2] = new double[10, 17];


            //CALCOLA I GRADIENTI DEI COLLEGAMENTI
            for (int i=0;i<17;i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    gradient[2][j, i] = OfflineGradient(3, j, i, e);
                }
            }

            for (int i = 0; i < 17; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    gradient[1][j, i] = OfflineGradient(2, j, i, e);
                }
            }

            for (int i = 0; i < 785; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    gradient[0][j, i] = OfflineGradient(1, j, i, e);
                }
            }



            //MODIFICA I COLLEGAMENTI DOPO AVER CALCOLATO TUTTI I GRADIENTI
            for (int i = 0; i < 17; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    weight[2][j, i] -= gradient[2][j, i];
                }
            }

            for (int i = 0; i < 17; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    weight[1][j, i] -= gradient[1][j, i];
                }
            }

            for (int i = 0; i < 785; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    weight[0][j, i] -= gradient[0][j, i];
                }
            }



        }

        private double OfflineGradient(int layerN, int j, int i, Example[] e)
        {
            
            double s = 0;
            for(int t = 0; t < e.Length; t++)
            {
                ForwardPropagate(e[t]);
                s += Math.Pow(_momentum, e.Length - t) * LocalGradient(layerN, j, e[t].y) * layer[layerN-1][i];
            }
            return _learningFactor * s;
        }

        private double LocalGradient(int layerN, int j, double[] y)
        {
            if (layerN == 3) //Neurone di output
            {
                return LocalError(j,y)*SigmoidDerivative(InverseSigmoid(layer[layerN][j]));
            }
            else
            {
                double s = 0;
                int N = layer[layerN + 1].Length;
                    for (int k = 0; k < N; k++) //Il layer di output parte da 0
                    {
                        s += LocalGradient(layerN + 1, k, y) * weight[layerN][k, j]; //COMPLESSITA' ELEVATA, TENTARE TECNICA DI MEMOIZZAZIONE
                    }
                return s * SigmoidDerivative(InverseSigmoid(layer[layerN][j]));
            }
        }
    
        private double LocalError(int j, double[] y)
        {
            //return layer[3][j] - y[j]; ATTENZIONE! ERRORE ALLA BASE. TESTARE FUNZIONAMENTO RETE DOPO IL CAMBIAMENTO
            return y[j] - layer[3][j];
        }

        private double TotalError(double[] y)
        {
            double s = 0.0;
            for(int j = 0; j < 10; j++)
            {
                s += Math.Pow(LocalError(j, y),2.0);
            }
            return s / 2.0;
        }

        //TODO
        private double CostFunction()
        {
            return 0;
        }

        public void Output()
        {
            for (int i = 0; i < 10; i++) Console.WriteLine(layer[3][i]);
        }

        public virtual double Sigmoid(double x)
        {
            double e = Math.E;
            double R;
            R = 1 / (1+Math.Pow(e,-x));
            return R;
        }

        public virtual double InverseSigmoid(double x)
        {
            return Math.Log(x / (1 - x));
        }

        public virtual double SigmoidDerivative(double x)
        {
            return Sigmoid(x) * (1 - Sigmoid(x));
        }

    }
}
