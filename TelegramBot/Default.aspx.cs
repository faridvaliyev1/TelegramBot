using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public partial class _Default : Page
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("1220381780:AAHvmN8TiPxUIQNVvjDCUptGWEn_hplrpdM");
        protected void Page_Load(object sender, EventArgs e)
        {
            Bot.OnMessage += Bot_OnMessage;

            Bot.OnMessageEdited += Bot_OnMessage;

            Bot.StartReceiving();
            
            
            
            

        }

        private static  void Bot_OnMessage(object sender,Telegram.Bot.Args.MessageEventArgs e)
        {

            
            if(e.Message.Type==Telegram.Bot.Types.Enums.MessageType.Text)
            {
                if (e.Message.Text == "/start")
                {
                    using (SqlConnection Conn = new SqlConnection())
                    {
                        Conn.ConnectionString = ConfigurationManager.ConnectionStrings["DBPATH"].ConnectionString;

                        SqlCommand Comm = new SqlCommand();

                        Comm.Connection = Conn;

                        Conn.Open();


                        Comm.Parameters.Add("@CHAT_ID", SqlDbType.NVarChar);
                        Comm.Parameters["@CHAT_ID"].Value = e.Message.Chat.Id;

                        Comm.Parameters.Add("@STATUS_ID", SqlDbType.Int);
                        Comm.Parameters["@STATUS_ID"].Value = 1;


                        Comm.CommandText = @"SELECT COUNT(*) FROM VENDOR_STACK_CLIENTS WHERE CHAT_ID=@CHAT_ID";

                        

                        if (Convert.ToInt32(Comm.ExecuteScalar()) == 0)
                        {

                            Comm.CommandText = @"INSERT INTO VENDOR_STACK_CLIENTS(CHAT_ID,STATUS_ID) VALUES (@CHAT_ID,@STATUS_ID)";

                            Comm.ExecuteNonQuery();
                            Bot.SendTextMessageAsync(e.Message.Chat.Id, "Please enter the name of the company");
                        }
                    }
                }
                else
                {
                    var rkm = new ReplyKeyboardMarkup();
                    var rows = new List<KeyboardButton[]>();
                    var cols = new List<KeyboardButton>();

                    cols.Add(new KeyboardButton("Add"));
                    cols.Add(new KeyboardButton("Remove me"));
                    rows.Add(cols.ToArray());
                   
                    rkm.Keyboard = rows.ToArray();

                    Bot.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Text, replyMarkup: rkm);

                    
                    using (SqlConnection Conn=new SqlConnection())
                    {
                        Conn.ConnectionString = ConfigurationManager.ConnectionStrings["DBPATH"].ConnectionString;

                        SqlCommand Comm = new SqlCommand();

                        Comm.Connection = Conn;

                        Conn.Open();

                        Comm.CommandText = @"SELECT ID FROM VENDORS WHERE LOGIN_SHORTNAME=@NAME";

                        Comm.Parameters.Add("@NAME", SqlDbType.NVarChar);
                        Comm.Parameters["@NAME"].Value = e.Message.Text;

                        SqlDataReader reader = Comm.ExecuteReader();

                        int VENDOR_ID = 0;

                        if (reader.Read())
                            VENDOR_ID = Convert.ToInt32(reader["ID"]);
                        else
                            VENDOR_ID = 0;

                        reader.Close();

                        if(VENDOR_ID>0)
                        {
                            Comm.Parameters.Clear();

                            Comm.CommandText = @"SELECT ID FROM VENDOR_STACK WHERE VENDOR_ID=@VENDOR_ID 
                                               AND CONVERT(VARCHAR(10),CREATE_DATE,121)=CONVERT(VARCHAR(10),DATEADD(HOUR,10,GETDATE()),121)";

                            Comm.Parameters.Add("@VENDOR_ID", SqlDbType.Int);
                            Comm.Parameters["@VENDOR_ID"].Value = VENDOR_ID;

                            reader = Comm.ExecuteReader();

                            int STACK_ID = 0;

                            if (reader.Read())
                            {
                                STACK_ID = Convert.ToInt32(reader["ID"]);
                            }
                            else
                            {
                                reader.Close();

                                Comm.CommandText= @"INSERT INTO VENDOR_STACK(VENDOR_ID,CREATE_DATE) VALUES (@VENDOR_ID,DATEADD(HOUR,10,GETDATE()));SELECT SCOPE_IDENTITY();";

                                STACK_ID = Convert.ToInt32(Comm.ExecuteScalar());
                            }

                            reader.Close();

                            Comm.Parameters.Clear();

                            Comm.CommandText = @"SELECT ID FROM VENDOR_STACK_CLIENTS WHERE CHAT_ID=@CHAT_ID AND STACK_ID=@STACK_ID AND STATUS_ID=@STATUS_ID";

                            Comm.Parameters.Add("@CHAT_ID", SqlDbType.Int);
                            Comm.Parameters["@CHAT_ID"].Value = e.Message.Chat.Id;

                            Comm.Parameters.Add("@STACK_ID", SqlDbType.Int);
                            Comm.Parameters["@STACK_ID"].Value = STACK_ID ;

                            Comm.Parameters.Add("@STATUS_ID", SqlDbType.Int);
                            Comm.Parameters["@STATUS_ID"].Value = 1;

                            reader = Comm.ExecuteReader();

                            int STACK_CLIENT_ID = 0;

                            if(reader.Read())
                            {
                                STACK_CLIENT_ID = Convert.ToInt32(reader["ID"]);
                            }
                            else
                            {
                                reader.Close();
                                Comm.CommandText = @"INSERT INTO VENDOR_STACK_CLIENTS(STACK_ID,CHAT_ID,STATUS_ID) VALUES (@STACK_ID,@CHAT_ID,@STATUS_ID);SELECT SCOPE_IDENTITY();";

                                STACK_CLIENT_ID = Convert.ToInt32(Comm.ExecuteScalar());


                            }
                            reader.Close();

                            Comm.Parameters.Clear();

                            Comm.CommandText = @"SELECT COUNT(*) FROM VENDOR_STACK_CLIENTS WHERE STACK_ID=@STACK_ID AND ID<=@ID";

                            Comm.Parameters.Add("@STACK_ID", SqlDbType.Int);
                            Comm.Parameters["@STACK_ID"].Value = STACK_ID;

                            Comm.Parameters.Add("@ID", SqlDbType.Int);
                            Comm.Parameters["@ID"].Value = STACK_CLIENT_ID;

                            int COUNT = Convert.ToInt32(Comm.ExecuteScalar());


                            Bot.SendTextMessageAsync(e.Message.Chat.Id, "Hi your order is {0}".Replace("{0}", (COUNT).ToString()));
                            
                            


                        }
                        else
                        {

                        }
                    }
                }

            }
        }
    }
}