using System;
using System.IO;
using System.Text;

public class ObjectTemplate {


    public string path;
    int version;
    bool isAbstract;

    TextReader reader;

    public List<string> parents;

    public Dictionary<string, string> stats;


    public ObjectTemplate(string baseFolder, string path) {

        this.path = path;
        parents = new List<string>();
        stats = new Dictionary<string, string>();

        reader = new StreamReader(File.OpenRead(Path.Combine(baseFolder, path)));

        string token = GetNextToken();
        while(token != null) {

            //Console.WriteLine("TOKEN " + token);

            if (token == "version") {
                version = int.Parse(GetNextToken());
                if (version != 2) Console.WriteLine($"VERSION {version}");
            } else if (token == "abstract") isAbstract = true;
            else if (token == "extends") {

                string parent = GetNextToken();
                //Console.WriteLine("PARENT " + parent);
                if (parent != "nothing") parents.Add(parent + ".ot");
            }

            else if (token == "Stats") {
                token = GetNextToken(); if(token != "{") {
                    Console.WriteLine(path + " Stats not followed by open bracket"); break;
                }

                token = GetNextToken();
                while(token != "}") {
                    string stat = token;
                    token = GetNextToken(); if (token != "=") {
                        Console.WriteLine(path + " " + stat + " Stat not followed by equals"); break;
                    }
                    stats[stat] = GetNextToken();
                    token = GetNextToken();
                }


            }



            token = GetNextToken();
        }

        /*

        while(reader.Peek() != -1) {
            string token = GetNextToken();

            if (token.StartsWith("//")) tokens.Clear(); // single line comment

            if(state == State.Toplevel) {
                if(token == "version") {
                    version = int.Parse(GetNextToken());
                    if (version != 2) Console.WriteLine($"VERSION {version}");
                } else if (token == "extends") {
                    parents.Add(GetNextToken());
                }

            }

        }
        */




        reader.Close();

    }


    string GetNextToken() {
        StringBuilder s = new StringBuilder();
        
        while (char.IsWhiteSpace((char)reader.Peek())) reader.Read(); //start whitespace
        if (reader.Peek() == -1) return null;
        int c = reader.Read();

        //single line comment
        if (c == '/' && ((char)reader.Peek()) == '/') {
            reader.ReadLine();
            //Console.WriteLine("COMMENT " + reader.ReadLine());
            return GetNextToken();
        }

        //string
        if (c == '"') {
            s.Append((char)c);
            do {
                c = reader.Read();
                s.Append((char)c);
            } while (c != '"');
            return s.ToString().Trim('"');
        }
        
        
        //regular token
        do {
            s.Append((char)c);
            c = reader.Read();
        } while (!char.IsWhiteSpace((char)c) && c != -1);
        return s.ToString();
    }

}