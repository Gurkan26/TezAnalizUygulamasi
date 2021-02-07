using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TezAnaliz
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Center;
       
        }

     
        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "URL KONTROL EDİLİYOR..." + Environment.NewLine;
            richTextBox1.Text = richTextBox1.Text + "BAŞLIKLAR KONTROL EDİLİYOR" + Environment.NewLine;
            richTextBox1.Text = richTextBox1.Text + "YAZI TİPİ KONTROL EDİLİYOR" + Environment.NewLine;
            richTextBox1.Text = richTextBox1.Text + "SAYFA DÜZENİ KONTROL EDİLİYOR" + Environment.NewLine;
            richTextBox1.Text = richTextBox1.Text + "----KONTROL İŞLEMİ TAMAMLANDI----" + Environment.NewLine;
            richTextBox1.Text = richTextBox1.Text + "__ _HATALAR_ __" + Environment.NewLine;
            Document doc = new Document();
            bool value = false;
            try
            {
                doc.LoadFromFile(@""+labelyol.Text);
                value = false;
            }
            catch
            {
                MessageBox.Show(".Docx uzantılı bir dosya giriniz.");
                value = true;
            }

            if (!value)
            {
                List<YaziTipiBilgisi> HeadInfos = new List<YaziTipiBilgisi>();
                List<YaziTipiBilgisi> ParaInfos = new List<YaziTipiBilgisi>();
                List<SayfaBilgisi> PageInfos = new List<SayfaBilgisi>();
                List<UrlBilgi> UrlInfos = new List<UrlBilgi>();



                HeadInfos.Clear();
                PageInfos.Clear();
                ParaInfos.Clear();
                UrlInfos.Clear();


                foreach (Section sec in doc.Sections)
                {
                   
                    SayfaBilgisi pageInfo = new SayfaBilgisi();
                    pageInfo.Left = sec.PageSetup.Margins.Left * 2.54f / 72f;
                    pageInfo.Right = sec.PageSetup.Margins.Right * 2.54f / 72f;
                    PageInfos.Add(pageInfo);

                    foreach (DocumentObject obj in sec.Body.ChildObjects)
                    {
                        if (obj.DocumentObjectType == DocumentObjectType.Paragraph)
                        {
                            Paragraph para = obj as Paragraph;
                            if (para.StyleName.Contains("Balk1"))   
                            {
                                foreach (DocumentObject paraObj in para.ChildObjects)
                                {
                                    if (paraObj.DocumentObjectType == DocumentObjectType.TextRange)
                                    {
                                        TextRange textRange = paraObj as TextRange;
                                        YaziTipiBilgisi head = new YaziTipiBilgisi();
                                        head.FontName = textRange.CharacterFormat.FontName;
                                        head.FontSize = textRange.CharacterFormat.FontSize;
                                        head.FontColor = textRange.CharacterFormat.TextColor;
                                        HeadInfos.Add(head);
                                    }
                                }
                            }
                            else if (!para.Text.Equals(""))           
                            {
                                foreach (DocumentObject paraObj in para.ChildObjects)
                                {
                                    if (paraObj.DocumentObjectType == DocumentObjectType.TextRange)
                                    {
                                        TextRange textRange = paraObj as TextRange;
                                        YaziTipiBilgisi parainfo = new YaziTipiBilgisi();
                                        parainfo.FontName = textRange.CharacterFormat.FontName;
                                        parainfo.FontSize = textRange.CharacterFormat.FontSize;
                                        parainfo.FontColor = textRange.CharacterFormat.TextColor;
                                        ParaInfos.Add(parainfo);

                                    }

                                    if (paraObj.DocumentObjectType == DocumentObjectType.Field)
                                    {
                                        Field field = paraObj as Field;
                                        if (field.Type.Equals(FieldType.FieldHyperlink))
                                        {
                                            UrlBilgi urlInfo = new UrlBilgi();
                                            urlInfo.Url = field.FieldText;
                                            UrlInfos.Add(urlInfo);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }


                string pattern = "[a-zA-z]+://[^\\s]*";
                Regex rgx = new Regex(pattern);
                TextSelection[] textSelections = doc.FindAllPattern(rgx);

                if (textSelections != null)
                {
                foreach (TextSelection textSelection in textSelections)
                    {
                        TextRange textRange = textSelection.GetAsOneRange();
                        UrlBilgi urlInfo = new UrlBilgi();
                        urlInfo.Url = textRange.Text;
                        UrlInfos.Add(urlInfo);
                    }

                }
             

                foreach (UrlBilgi urlInfo in UrlInfos)
                {
                    string url = urlInfo.Url;
                    HttpWebRequest req = null;
                    try
                    {
                        req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                        req.Method = "HEAD";
                        req.Timeout = 10000;
                        HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                        if (Convert.ToInt32(res.StatusCode).ToString().Equals("200"))
                        {
                            urlInfo.Message = "Url çalışıyor";
                        }
                    }
                    catch (Exception ex)
                    {
                        urlInfo.Message = ex.Message;
                    }
                    finally
                    {
                        if (req != null)
                        {
                            req.Abort();
                            req = null;
                        }
                    }
                }
            


                bool urlValue = false;
                foreach (UrlBilgi urlInfo in UrlInfos)
                {
                    if (!urlInfo.Message.Equals("URL ÇALIŞIYOR"))
                    {
                        urlValue = true;
                    }
                }
                if (urlValue)
                {
                   richTextBox1.Text = richTextBox1.Text + "URL TABLOSUNU KONTROL EDİNİZ " +Environment.NewLine;
                }

                bool paraValue = false;
                bool paraValueSize = false;
                foreach (YaziTipiBilgisi fontInfo in ParaInfos)
                {
                    if (!fontInfo.FontName.Equals("Times New Roman"))
                    {
                        paraValue = true;
                    }
                    if (!fontInfo.FontSize.Equals("12"))

                    {
                        paraValueSize = true;
                    }
                   
                }
                if (paraValue)
                {
                   richTextBox1.Text = richTextBox1.Text+"Paragraflar'da Yazı tipini 'Times New Roman' olarak değiştirin. " + Environment.NewLine;
                }
                if (paraValueSize)
                {
                    richTextBox1.Text = richTextBox1.Text + "Paragraflar'da Yazı boyutunu '12' olarak değiştirin." + Environment.NewLine;
                }
             
                bool headValue = false;
                bool headValueSize = false;
                foreach (YaziTipiBilgisi fontInfo in HeadInfos)
                {
                    if (!fontInfo.FontName.Equals("Times New Roman"))
                    {
                        headValue = true;
                    }
                    if (!fontInfo.FontSize.Equals("14"))
                    {
                        headValueSize = true;
                    }
                }
                if (headValue)
                {
                    richTextBox1.Text = richTextBox1.Text + "Başlık'ta Yazı tipini 'Times New Roman' olarak değiştirin. " + Environment.NewLine;
                }
               if (headValueSize)
                {
                    richTextBox1.Text = richTextBox1.Text + "Başlık'ta Yazı boyutunu '14' olarak değiştirin." + Environment.NewLine;
                }
            }


           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            //file.Filter = "Word Dosyası |*.xml| Word Dosyası|*.xml";  
            file.FilterIndex = 2;
            file.RestoreDirectory = true;
            file.CheckFileExists = false;
            file.Title = "Docx Formatındaki Dosyası Seçiniz..";
            file.Multiselect = true;
            // bu kod çoklu seçim yapabilmemizi sağlar.

            if (file.ShowDialog() == DialogResult.OK)
            {
                string DosyaYolu = file.FileName;
                string DosyaAdi = file.SafeFileName;
                labelyol.Text = DosyaYolu;
            }
            

        }
    }

    public class YaziTipiBilgisi
    {
        public string FontName { get; set; }
        public float FontSize { get; set; }
        public Color FontColor { get; set; }
    }

    public class SayfaBilgisi
    {
        public float Left { get; set; }
        public float Right { get; set; }
    }

    public class UrlBilgi
    {
        public string Url { get; set; }
        public string Message { get; set; }
    }

}
