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

    public static void DumpTokens(string path) {
        TextReader reader = new StreamReader(path);
        string token = GetNextToken(reader);
        while(token is not null) {
            Console.WriteLine(token);
            token = GetNextToken(reader);
        }
    }

    public ObjectTemplate(string baseFolder, string path) {

        this.path = path;
        parents = new List<string>();
        stats = new Dictionary<string, string>();

        if(!File.Exists(Path.Combine(baseFolder, path))) {
            Console.WriteLine(path + " DOES NOT EXIST");
            return;
        }

        TextReader reader = new StreamReader(File.OpenRead(Path.Combine(baseFolder, path)));

        string token = GetNextToken(reader);
        while(token != null) {

            //Console.WriteLine("TOKEN " + token);

            if (token == "version") {
                version = int.Parse(GetNextToken(reader));
                if (version != 2) Console.WriteLine($"VERSION {version}");
            } else if (token == "abstract") isAbstract = true;
            else if (token == "extends") {

                string parent = GetNextToken(reader);
                //Console.WriteLine("PARENT " + parent);
                if (parent != "nothing") parents.Add(parent + ".ot");
            }

            else if (token == "Stats") {
                token = GetNextToken(reader); if(token != "{") {
                    Console.WriteLine(path + " Stats not followed by open bracket"); break;
                }

                token = GetNextToken(reader);
                while(token != "}") {
                    string stat = token;
                    token = GetNextToken(reader); if (token != "=") {
                        Console.WriteLine(path + " " + stat + " Stat not followed by equals"); break;
                    }
                    stats[stat] = GetNextToken(reader);
                    token = GetNextToken(reader);
                }


            }



            token = GetNextToken(reader);
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


    static string GetNextToken(TextReader reader) {
        StringBuilder s = new StringBuilder();
        
        while (char.IsWhiteSpace((char)reader.Peek())) reader.Read(); //start whitespace
        if (reader.Peek() == -1) return null;
        int c = reader.Read();

        //single line comment
        if (c == '/' && ((char)reader.Peek()) == '/') {
            reader.ReadLine();
            //Console.WriteLine("COMMENT " + reader.ReadLine());
            return GetNextToken(reader);
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