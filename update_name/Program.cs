using System;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using CoreTweet;
using CoreTweet.Streaming;

namespace update_name
{
    class Program
    {
        private static XmlSerializer x_ = null;
        private static XmlSerializer x
        {
            get
            {
                if (x_ == null)
                {
                    x_ = new XmlSerializer(typeof(Tokens));
                }
                return x_;
            }
        }
        private static Tokens t_ = null;
        private static Tokens t
        {
            get
            {
                if (t_ == null)
                {
                    if (File.Exists("bot.xml"))
                    {
                        using (var y = File.OpenRead("bot.xml"))
                        {
                            t_ = x.Deserialize(y) as Tokens;
                        }
                    }
                    else
                    {
                        var se = OAuth.Authorize("JLHxvWliaKG7YxCCrE9TXw", "r3p7efH2IEa7Po44wajANJ5r3OL0QN9sAjbB6mgZKM");
                        Console.WriteLine(se.AuthorizeUri);
                        Console.WriteLine("pin> ");
                        t_ = se.GetTokens(Console.ReadLine());
                        using (var y = File.Open("bot.xml", FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            x.Serialize(y, t_);
                        }
                    }
                }
                return t_;
            }
        }
        private static Regex pattern1_ = null;
        private static Regex pattern1
        {
            get
            {
                if (pattern1_ == null)
                {
                    pattern1_ = new Regex("^@[a-zA-Z0-9_]+[ ]+update_name[ ]+(.+)$");
                }
                return pattern1_;
            }
        }
        private static Regex pattern2_ = null;
        private static Regex pattern2
        {
            get
            {
                if (pattern2_ == null)
                {
                    pattern2_ = new Regex(@"^(.+)[\(（]@[a-zA-Z0-9_]+[\)）]$");
                }
                return pattern2_;
            }
        }
        private static void parse(Status s)
        {
            var match1 = pattern1.Match(s.Text);
            var match2 = pattern2.Match(s.Text);
            var match = match1.Success ? match1 : match2;
            if (match.Success)
            {
                t.Account.UpdateProfile(name => match.Groups[1].Value);
                t.Statuses.Update(status => s.User.Name + "氏(@" + s.User.ScreenName + ")により「" + match.Groups[1].Value + "」に改名しました！", in_reply_to_status_id => s.ID);
            }
        }
        private static void reply(Status s)
        {
            try
            {
                parse(s);
            }
            catch (Exception e)
            {
                Console.WriteLine("err: failed to send at " + DateTime.Now.ToString() + ", to " + s.ID.ToString() + "\n" + e.ToString());
            }
        }
        static void Main(string[] args)
        {
            try
            {
                foreach (var m in t.Streaming.StartStream(StreamingType.Public, new StreamingParameters(track => "@" + t.Account.VerifyCredentials().ScreenName)))
                {
                    if (m is StatusMessage)
                    {
                        reply((m as StatusMessage).Status);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
