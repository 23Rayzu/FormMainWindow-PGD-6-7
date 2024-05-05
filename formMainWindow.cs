using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using AxMapWinGIS;
using MapWinGIS;
using System.Threading.Tasks;
using MWLite.Symbology.LegendControl;

namespace PGD_6_7
{
    public partial class formMainWindow : Form
    {

        public static string strAppPath = "Application.StartupPath";
        public static string strFilePath = Directory.GetParent(Directory.GetParent(strAppPath).ToString()).ToString() + "\\Resources";
        public int handleBatasKec;
        public int handleNamaAset;
        public FormPopUp formPopUpObject = null;
        public Addpoint formAddPointObject = null;
        public Buffer formBufferObject = null;
        public bool sedangload;

        public formMainWindow()
        {
            InitializeComponent();
            strAppPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            strFilePath = Path.Combine(strAppPath, "Resources");
            Legend1.Map = (MapWinGIS.Map)axMap1.GetOcx();
            axMap1.SendMouseMove = true;
            axMap1.SendMouseDown = true;
            axMap1.SendMouseUp = true;
            axMap1.MapCursor = tkCursor.crsrMapDefault;
            KryptonRibbonGroupButton_Identify.PerformClick();
            formPopUpObject = new FormPopUp(this);
            formAddPointObject = new Addpoint(this);
            formBufferObject = new Buffer(this);
            Legend1.Map = (Map)axMap1.GetOcx();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var utils = new Utils();

            //==============================
            //ADD LAYER BATAS ADMIN
            //==============================
            string shpBatasKec = Path.Combine(strFilePath, @"D:\SIG MUKHLISHSemester 2\Prak PGD\acara 6 dan 7\Resources\Spatial\ADMINISTRASIKECAMATAN_AR_50K.shp");
            MapWinGIS.Shapefile sfBatasKec = new MapWinGIS.Shapefile();
            sfBatasKec.Open(shpBatasKec, null);
            handleBatasKec = Legend1.Layers.Add(sfBatasKec, true);
            Legend1.GetLayer(handleBatasKec).Name = "Batas Administrasi";

            int fidDesa = sfBatasKec.Table.get_FieldIndexByName("desa_kel");
            sfBatasKec.Categories.Generate(fidDesa, tkClassificationType.ctUniqueValues, 0);
            ColorScheme schemeBatasKec = new ColorScheme();

            //sfBatasDesa.Categories.ApplyColorScheme(tkColorSchemeType.ctSchemeGraduate, schemeBatasDesa);
            schemeBatasKec.SetColors2(tkMapColor.OrangeRed, tkMapColor.LightYellow);
            sfBatasKec.Categories.ApplyColorScheme3(tkColorSchemeType.ctSchemeGraduated,
                schemeBatasKec, tkShapeElements.shElementFill, 0, Convert.ToInt32(sfBatasKec.Categories.Count / 2));

            schemeBatasKec.SetColors2(tkMapColor.ForestGreen, tkMapColor.PowderBlue);
            sfBatasKec.Categories.ApplyColorScheme3(tkColorSchemeType.ctSchemeGraduated,
                schemeBatasKec, tkShapeElements.shElementFill, (Convert.ToInt32(sfBatasKec.Categories.Count / 2) + 1),
                (sfBatasKec.Categories.Count - 1));
            axMap1.Redraw();





            //==============================
            //ADD LAYER JARINGAN JALAN
            //==============================
            string shpJalan = Path.Combine(strFilePath, @"D:\SIG MUKHLISHSemester 2\Prak PGD\acara 6 dan 7\Resources\Spatial\JALAN_LN_50K.shp");
            MapWinGIS.Shapefile sfJalan = new MapWinGIS.Shapefile();
            sfJalan.Open(shpJalan, null);
            int handleJalan = Legend1.Layers.Add(sfJalan, true);
            Legend1.GetLayer(handleJalan).Name = "Jaringan Jalan";

            LinePattern patternKolektor = new LinePattern();
            patternKolektor.AddLine(utils.ColorByName(tkMapColor.Red), 4.0f, tkDashStyle.dsDashDot);
            ShapefileCategory ctKolektor = sfJalan.Categories.Add("Jalan Kolektor");
            ctKolektor.Expression = "[REMARK] = \"Jalan Kolektor\"";
            ctKolektor.DrawingOptions.LinePattern = patternKolektor;
            ctKolektor.DrawingOptions.UseLinePattern = true;

            LinePattern patternLokal = new LinePattern();
            patternLokal.AddLine(utils.ColorByName(tkMapColor.Red), 3.0f, tkDashStyle.dsDashDot);
            ShapefileCategory ctLokal = sfJalan.Categories.Add("Jalan Lokal");
            ctLokal.Expression = "[REMARK] = \"Jalan Lokal\"";
            ctLokal.DrawingOptions.LinePattern = patternLokal;
            ctLokal.DrawingOptions.UseLinePattern = true;

            LinePattern patternLain = new LinePattern();
            patternLain.AddLine(utils.ColorByName(tkMapColor.Red), 2.0f, tkDashStyle.dsDashDot);
            ShapefileCategory ctLain = sfJalan.Categories.Add("Jalan Lain");
            ctLain.Expression = "[REMARK] = \"Jalan Lain\"";
            ctLain.DrawingOptions.LinePattern = patternLain;
            ctLain.DrawingOptions.UseLinePattern = true;

            sfJalan.DefaultDrawingOptions.Visible = false; // hide all the unclassified points
            sfJalan.Categories.ApplyExpressions();
            axMap1.Redraw();


            //==============================
            //ADD LAYER Sarana Ibadah
            //==============================
            loadNamaAset();
            {
                string shpNamaAset = Path.Combine(strFilePath, @"D:\SIG MUKHLISHSemester 2\Prak PGD\acara 6 dan 7\Resources\Spatial\PEMERINTAHAN_PT_50K.shp");
                MapWinGIS.Shapefile sfNamaAset = new MapWinGIS.Shapefile();
                sfNamaAset.Open(shpNamaAset, null);
                {
                    MapWinGIS.Image imgkntorcmat = new MapWinGIS.Image();
                    imgkntorcmat.Open(Path.Combine(strFilePath, @"D:\SIG MUKHLISHSemester 2\Prak PGD\acara 6 dan 7\Resources\Spatial\KntorCmat.png"),
                    ImageType.USE_FILE_EXTENSION, true, null);
                    ShapefileCategory ctKades = sfNamaAset.Categories.Add("kntorcmat");
                    ctKades.Expression = "[REMARK] = \"kntorcmat\"";
                    ctKades.DrawingOptions.PointType = tkPointSymbolType.ptSymbolPicture;
                    ctKades.DrawingOptions.Picture = imgkntorcmat;
                    ctKades.DrawingOptions.PictureScaleX = 0.25;
                    ctKades.DrawingOptions.PictureScaleY = 0.25;

                    MapWinGIS.Image imgkntrlrah = new MapWinGIS.Image();
                    imgkntrlrah.Open(Path.Combine(strFilePath, @"D:\SIG MUKHLISHSemester 2\Prak PGD\acara 6 dan 7\Resources\Spatial\KntorLrah.png"),
                        ImageType.USE_FILE_EXTENSION, true, null);
                    ShapefileCategory ctkntrlrah = sfNamaAset.Categories.Add("kntrlrah");
                    ctkntrlrah.Expression = "[REMARK] = \"kntrlrah\"";
                    ctkntrlrah.DrawingOptions.PointType = tkPointSymbolType.ptSymbolPicture;
                    ctkntrlrah.DrawingOptions.Picture = imgkntrlrah;
                    ctkntrlrah.DrawingOptions.PictureScaleX = 0.25;
                    ctkntrlrah.DrawingOptions.PictureScaleY = 0.25;

                    MapWinGIS.Image imgkntrwlkt = new MapWinGIS.Image();
                    imgkntrwlkt.Open(Path.Combine(strFilePath, @"D:\SIG MUKHLISHSemester 2\Prak PGD\acara 6 dan 7\Resources\Spatial\KntorWlkt.png"),
                        ImageType.USE_FILE_EXTENSION, true, null);
                    ShapefileCategory ctkntrwlkt = sfNamaAset.Categories.Add("kntrwlkt");
                    ctkntrwlkt.Expression = "[REMARK] = \"kntrwlkt \"";
                    ctkntrwlkt.DrawingOptions.PointType = tkPointSymbolType.ptSymbolPicture;
                    ctkntrwlkt.DrawingOptions.Picture = imgkntrwlkt;
                    ctkntrwlkt.DrawingOptions.PictureScaleX = 0.25;
                    ctkntrwlkt.DrawingOptions.PictureScaleY = 0.25;

                    sfNamaAset.DefaultDrawingOptions.Visible = false; // hide all the unclasified points
                    sfNamaAset.Categories.ApplyExpressions();
                    axMap1.Redraw();
                }
            }
        }

        //==============================
        //BASEMAP
        //==============================
        private void Basemap_None_CheckedChanged_1(object sender, EventArgs e)
        {
            axMap1.TileProvider = MapWinGIS.tkTileProvider.ProviderNone;
            axMap1.Redraw();
            axMap1.Refresh();
        }

        private void Basemap_OpenStreetMap_CheckedChanged_1(object sender, EventArgs e)
        {
            axMap1.TileProvider = MapWinGIS.tkTileProvider.OpenStreetMap;
            axMap1.Redraw();
            axMap1.Refresh();
        }

        private void Basemap_OpenCycleMap_CheckedChanged_1(object sender, EventArgs e)
        {
            axMap1.TileProvider = MapWinGIS.tkTileProvider.OpenCycleMap;
            axMap1.Redraw();
            axMap1.Refresh();
        }

        private void Basemap_OpenTransportMap_CheckedChanged_1(object sender, EventArgs e)
        {
            axMap1.TileProvider = MapWinGIS.tkTileProvider.OpenTransportMap;
            axMap1.Redraw();
            axMap1.Refresh();
        }

        private void Basemap_BingMaps_CheckedChanged_1(object sender, EventArgs e)
        {
            axMap1.TileProvider = MapWinGIS.tkTileProvider.BingMaps;
            axMap1.Redraw();
            axMap1.Refresh();
        }

        private void Basemap_BingSatellite_CheckedChanged_1(object sender, EventArgs e)
        {
            axMap1.TileProvider = MapWinGIS.tkTileProvider.BingSatellite;
            axMap1.Redraw();
            axMap1.Refresh();
        }

        private void Basemap_BingHybrid_CheckedChanged_1(object sender, EventArgs e)
        {
            axMap1.TileProvider = MapWinGIS.tkTileProvider.BingHybrid;
            axMap1.Redraw();
            axMap1.Refresh();
        }

        private void Basemap_GoogleMaps_CheckedChanged_1(object sender, EventArgs e)
        {
            axMap1.TileProvider = MapWinGIS.tkTileProvider.GoogleMaps;
            axMap1.Redraw();
            axMap1.Refresh();
        }

        private void Basemap_GoogleSatellite_CheckedChanged_1(object sender, EventArgs e)
        {
            axMap1.TileProvider = MapWinGIS.tkTileProvider.GoogleSatellite;
            axMap1.Redraw();
            axMap1.Refresh();
        }

        private void Basemap_GoogleHybrid_CheckedChanged_1(object sender, EventArgs e)
        {
            axMap1.TileProvider = MapWinGIS.tkTileProvider.GoogleHybrid;
            axMap1.Redraw();
            axMap1.Refresh();
        }

        private void Basemap_GoogleTerrain_CheckedChanged_1(object sender, EventArgs e)
        {
            axMap1.TileProvider = MapWinGIS.tkTileProvider.GoogleTerrain;
            axMap1.Redraw();
            axMap1.Refresh();
        }

        private void Basemap_Rosreestr_CheckedChanged_1(object sender, EventArgs e)
        {
            axMap1.TileProvider = MapWinGIS.tkTileProvider.Rosreestr;
            axMap1.Redraw();
            axMap1.Refresh();
        }

        //==============================
        //CURSOR MODE, IDENTIFY, MEASURE
        //==============================
        private void KryptonRibbonGroup_NormalMode_Click(object sender, EventArgs e)
        {
            axMap1.MapCursor = tkCursor.crsrMapDefault;
            if (KryptonRibbonGroupButton_Normal.Checked == true)
            {
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmNone;
                KryptonRibbonGroupButton_ZoomIn.Checked = false;
                KryptonRibbonGroupButton_ZoomOut.Checked = false;
                KryptonRibbonGroupButton_Pan.Checked = false;
                KryptonRibbonGroupButton_Identify.Checked = false;
                KryptonRibbonGroupButton_Length.Checked = false;
                KryptonRibbonGroupButton_Area.Checked = false;
            }
            else
            {
                KryptonRibbonGroupButton_Normal.Checked = true;
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmNone;
            }
        }

        private void KryptonRibbonGroupButton_ZoomIn_Click(object sender, EventArgs e)
        {
            axMap1.MapCursor = tkCursor.crsrMapDefault;
            if (KryptonRibbonGroupButton_ZoomIn.Checked == true)
            {
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmZoomIn;
                KryptonRibbonGroupButton_Normal.Checked = false;
                KryptonRibbonGroupButton_ZoomOut.Checked = false;
                KryptonRibbonGroupButton_Pan.Checked = false;
                KryptonRibbonGroupButton_Identify.Checked = false;
                KryptonRibbonGroupButton_Length.Checked = false;
                KryptonRibbonGroupButton_Area.Checked = false;
            }
            else
            {
                KryptonRibbonGroupButton_Normal.Checked = true;
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmNone;
            }
        }

        private void KryptonRibbonGroupButton_ZoomOut_Click(object sender, EventArgs e)
        {
            axMap1.MapCursor = tkCursor.crsrMapDefault;
            if (KryptonRibbonGroupButton_ZoomOut.Checked == true)
            {
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmZoomOut;
                KryptonRibbonGroupButton_Normal.Checked = false;
                KryptonRibbonGroupButton_ZoomIn.Checked = false;
                KryptonRibbonGroupButton_Pan.Checked = false;
                KryptonRibbonGroupButton_Identify.Checked = false;
                KryptonRibbonGroupButton_Length.Checked = false;
                KryptonRibbonGroupButton_Area.Checked = false;
            }
            else
            {
                KryptonRibbonGroupButton_Normal.Checked = true;
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmNone;
            }
        }

        private void KryptonRibbonGroupButton_Pan_Click(object sender, EventArgs e)
        {
            axMap1.MapCursor = tkCursor.crsrMapDefault;
            if (KryptonRibbonGroupButton_Pan.Checked == true)
            {
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmPan;
                KryptonRibbonGroupButton_Normal.Checked = false;
                KryptonRibbonGroupButton_ZoomIn.Checked = false;
                KryptonRibbonGroupButton_ZoomOut.Checked = false;
                KryptonRibbonGroupButton_Identify.Checked = false;
                KryptonRibbonGroupButton_Length.Checked = false;
                KryptonRibbonGroupButton_Area.Checked = false;
            }
            else
            {
                KryptonRibbonGroupButton_Normal.Checked = true;
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmNone;
            }
        }
        private void KryptonRibbonGroupButton_Identify_Click(object sender, EventArgs e)
        {
            axMap1.MapCursor = tkCursor.crsrMapDefault;
            if (KryptonRibbonGroupButton_Identify.Checked == true)
            {
                axMap1.MapCursor = tkCursor.crsrUpArrow;
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmNone;
                KryptonRibbonGroupButton_Normal.Checked = false;
                KryptonRibbonGroupButton_ZoomIn.Checked = false;
                KryptonRibbonGroupButton_ZoomOut.Checked = false;
                KryptonRibbonGroupButton_Pan.Checked = false;
                KryptonRibbonGroupButton_Length.Checked = false;
                KryptonRibbonGroupButton_Area.Checked = false;
            }
            else
            {
                KryptonRibbonGroupButton_Normal.Checked = true;
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmNone;
            }
        }

        private void KryptonRibbonGroupButton_Length_Click(object sender, EventArgs e)
        {
            axMap1.MapCursor = tkCursor.crsrMapDefault;
            if (KryptonRibbonGroupButton_Length.Checked == true)
            {
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmMeasure;
                axMap1.Measuring.MeasuringType = tkMeasuringType.MeasureDistance;
                KryptonRibbonGroupButton_Normal.Checked = false;
                KryptonRibbonGroupButton_ZoomIn.Checked = false;
                KryptonRibbonGroupButton_ZoomOut.Checked = false;
                KryptonRibbonGroupButton_Pan.Checked = false;
                KryptonRibbonGroupButton_Identify.Checked = false;
                KryptonRibbonGroupButton_Area.Checked = false;
            }
            else
            {
                KryptonRibbonGroupButton_Normal.Checked = true;
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmNone;
            }
        }

        private void KryptonRibbonGroupButton_Area_Click(object sender, EventArgs e)
        {
            axMap1.MapCursor = tkCursor.crsrMapDefault;
            if (KryptonRibbonGroupButton_Area.Checked == true)
            {
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmMeasure;
                axMap1.Measuring.MeasuringType = tkMeasuringType.MeasureArea;
                KryptonRibbonGroupButton_Normal.Checked = false;
                KryptonRibbonGroupButton_ZoomIn.Checked = false;
                KryptonRibbonGroupButton_ZoomOut.Checked = false;
                KryptonRibbonGroupButton_Pan.Checked = false;
                KryptonRibbonGroupButton_Identify.Checked = false;
                KryptonRibbonGroupButton_Length.Checked = false;
            }
            else
            {
                KryptonRibbonGroupButton_Normal.Checked = true;
                axMap1.CursorMode = MapWinGIS.tkCursorMode.cmNone;
            }
        }



        //==============================
        //CURSOR MODE, IDENTIFY, MEASURE
        //==============================
        private void KryptonRibbonGroupButton_ZoomIn1_Click(object sender, EventArgs e)
        {
            axMap1.ZoomIn(0.2);
        }

        private void KryptonRibbonGroupButton_ZoomOut1_Click(object sender, EventArgs e)
        {
            axMap1.ZoomOut(0.2);
        }

        private void KryptonRibbonGroupButton_FullExtent_Click(object sender, EventArgs e)
        {
            axMap1.ZoomToMaxExtents();
        }

        private void KryptonRibbonGroupButton_Prev_Click(object sender, EventArgs e)
        {
            axMap1.ZoomToPrev();
        }

        //==============================
        //DATA
        //==============================
        private void KryptonRibbonGroupButton_Add_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string strfileshp = "Shapefile belum dimasukkan Silahkan Pilih File Shp";

            ofd.Title = "Browse Shapefile";
            ofd.InitialDirectory = @"D:\";
            ofd.Filter = "Shapefile (*.shp)|*.shp|All files (*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;

            if ((ofd.ShowDialog() == DialogResult.OK))
            {
                strFilePath = ofd.FileName;

                MapWinGIS.Shapefile sfAsetBandarLampung = new MapWinGIS.Shapefile();
                sfAsetBandarLampung.Open(strfileshp, null);
                int handleBufferResult = Legend1.Layers.Add(sfAsetBandarLampung, true);
                Legend1.GetLayer(handleBufferResult).Name = System.IO.Path.GetFileName(strFilePath);
                sfAsetBandarLampung.Identifiable = true;

                if (!formBufferObject.cboInput.Items.Contains(strfileshp))
                {
                    formBufferObject.cboInput.Items.Add(strfileshp);
                }
                formBufferObject.cboInput.Text = strfileshp;
            }
            else
            {
                MessageBox.Show(strfileshp, "Report",
                    MessageBoxButtons.OKCancel);
            }
        }

        private void KryptonRibbonGroupButton_Remove_Click(object sender, EventArgs e)
        {
            Legend1.Layers.Remove(Legend1.SelectedLayer);
        }



        //==============================
        //DATAGRIDVIEW EVENT
        //==============================

        private void DataGridView1_RowHeaderMouseDoubleClick(object sender, EventArgs e)
        {
            if (DataGridView1.SelectedRows.Count > 0)
            {
                Shapefile sf = axMap1.get_Shapefile(handleNamaAset);
                sf.SelectNone();

                for (int i = 0; i < DataGridView1.SelectedRows.Count; i++)
                {
                    sf.set_ShapeSelected(Convert.ToInt32(DataGridView1.SelectedRows[i].Cells["fid"].Value), true);
                }
                axMap1.ZoomToSelected(handleNamaAset);
            }
        }

        private void DataGridView1_SelectionChanged_1(object sender, EventArgs e)
        {
            if (DataGridView1.SelectedRows.Count > 0)
            {
                Shapefile sf = axMap1.get_Shapefile(handleNamaAset);
                sf.SelectNone();

                for (int i = 0; i < DataGridView1.SelectedRows.Count; i++)
                {
                    sf.set_ShapeSelected(Convert.ToInt32(DataGridView1.SelectedRows[i].Cells["fid"].Value), true);
                }
                axMap1.ZoomToSelected(handleNamaAset);
            }
        }


        //==============================
        //QUERY
        //==============================
        private void KryptonRibbonGroupComboBoxQueryKecamatan_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(KryptonRibbonGroupComboBox_Kecamatan.Text == "Cari Kecamatan...")
                && !(KryptonRibbonGroupComboBox_Aset.Text == "Cari KantorPemerintah..."))
            {
                //Shapefile sfBatasDesa.....

                Shapefile sfNamaAset = axMap1.get_Shapefile(handleNamaAset);
                sfNamaAset.SelectNone();

                string errorNamaAset = "";
                object resultNamaAset = null;
                string queryNamaAset = "[REMARK] = \"" + KryptonRibbonGroupComboBox_Kecamatan.Text
                    + "\" AND [Nama_KP] = \"" + KryptonRibbonGroupComboBox_Aset.Text + "\"";

                if (sfNamaAset.Table.Query(queryNamaAset, ref resultNamaAset, ref errorNamaAset))
                {
                    int[] shapesNamaAset = resultNamaAset as int[];
                    if (shapesNamaAset != null)
                    {
                        for (int i = 0; i < shapesNamaAset.Length; i++)
                        {
                            sfNamaAset.set_ShapeSelected(shapesNamaAset[i], true);
                        }
                        axMap1.ZoomToSelected(handleNamaAset);
                        axMap1.ZoomIn(0.2);
                        axMap1.ZoomIn(0.2);
                        axMap1.ZoomIn(0.2);
                        axMap1.ZoomIn(0.2);
                        axMap1.ZoomIn(0.2);
                    }
                }
            }
        }

        private void KryptonRibbonGroupComboBox_Desa_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void KryptonRibbonGroupComboBox_Aset_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(KryptonRibbonGroupComboBox_Kecamatan.Text == "Cari Kecamatan..."))
            {
                KryptonRibbonGroupComboBox_Aset.Items.Clear();
                KryptonRibbonGroupComboBox_Aset.Text = "Cari Sarana Ibadah...";

                ///tanpa batas desa 

                Shapefile sfNamaAset = axMap1.get_Shapefile(handleNamaAset);
                sfNamaAset.SelectNone();

                string errorNamaAset = "";
                object resultNamaAset = null;
                string queryNamaAset = "[REMARK] = \"" + KryptonRibbonGroupComboBox_Kecamatan.Text + "\"";

                if (sfNamaAset.Table.Query(queryNamaAset, ref resultNamaAset, ref errorNamaAset))
                {
                    int[] shapesNamaAset = resultNamaAset as int[];
                    if (shapesNamaAset != null)
                    {
                        if (!(shapesNamaAset.Length == 0))
                        {
                            MessageBox.Show("Pada Kecamatan " + KryptonRibbonGroupComboBox_Kecamatan.Text
                                + "ditemukan " + shapesNamaAset.Length.ToString()
                                + "Sarana Ibadah Pemerintah Kota Solo..", "Informasi Sarana Ibadah", MessageBoxButtons.OK);
                        }
                        else
                        {
                            MessageBox.Show("Pada Kecamatan " + KryptonRibbonGroupComboBox_Kecamatan.Text
                               + " tidak ditemukan Sarana Ibadah  Pemerintah Kota Solo..",
                               "Informasi Sarana Ibadah", MessageBoxButtons.OK);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Pada Kecamatan " + KryptonRibbonGroupComboBox_Kecamatan.Text
                              + " tidak ditemukan Sarana Ibadah pemerintah Kota Solo..",
                              "Informasi Sarana Ibadah", MessageBoxButtons.OK);
                    }
                }
            }

        }




        //==============================
        //EDIT
        //==============================
        private void KryptonRibbonGroupButton_AddPoint_Click(object sender, EventArgs e)
        {
            formAddPointObject.Show();
            formAddPointObject.BringToFront();
        }

        //==============================
        //ANALYST
        //==============================
        private void KryptonRibbonGroupButton_Buffer_Click(object sender, EventArgs e)
        {
            formBufferObject.Show();
            formBufferObject.BringToFront();
        }

        //==============================
        //MAP EVENT
        //==============================
        private void Map1_MouseEvent(object sender, _DMapEvents_MouseUpEvent e)
        {
            double projX = 0.0;
            double projY = 0.0;
            axMap1.PixelToProj(e.x, e.y, ref projX, ref projY);
            object result = null;
            Extents ext = new Extents();
            ext.SetBounds(projX, projY, 0.0, projX, projY, 0.0);
            double tolerance = 100; //meters
            Utils utils = new Utils();
            utils.ConvertDistance(tkUnitsOfMeasure.umMeters, tkUnitsOfMeasure.umDecimalDegrees, ref tolerance);

            if (KryptonRibbonGroupButton_Identify.Checked == true)
            {
                Shapefile sf = axMap1.get_Shapefile(handleNamaAset);
                sf.SelectNone();
                axMap1.Redraw2(tkRedrawType.RedrawAll);
                axMap1.Refresh();

                formPopUpObject.Hide();
                if (sf is null)
                {
                    if (sf.SelectShapes(ext, tolerance, SelectMode.INTERSECTION, ref result))
                    {
                        int[] shapes = result as int[];
                        if (shapes.Length > 0)
                        {
                            sf.SelectNone();
                            sf.set_ShapeSelected(shapes[0], true);
                            axMap1.Redraw2(tkRedrawType.RedrawAll);
                            axMap1.Refresh();

                            formPopUpObject.txtShapeIndex.Text = shapes[0].ToString();


                            formPopUpObject.txtNamaAset.Text = sf.get_CellValue(
                                sf.Table.get_FieldIndexByName("nama kantor pemerintah"), shapes[0]).ToString();
                            formPopUpObject.cboTingkatAdmin.Text = sf.get_CellValue(
                                sf.Table.get_FieldIndexByName("tingkat administrasi"), shapes[0]).ToString();
                            formPopUpObject.txtFoto.Text = sf.get_CellValue(
                                sf.Table.get_FieldIndexByName("foto"), shapes[0]).ToString();

                            formPopUpObject.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                            formPopUpObject.pictureBox1.ImageLocation = Path.Combine(strFilePath,
                                @"D:\SIG MUKHLISH\Semester 2\Prak PGD\acara 6 dan 7\Resources\Non - Spatial" + sf.get_CellValue(
                                 sf.Table.get_FieldIndexByName("foto"), shapes[0]).ToString());

                            formPopUpObject.Show();
                            formPopUpObject.BringToFront();

                        }

                    }

                }

            }
            else if (axMap1.MapCursor == tkCursor.crsrCross)
            {
                formAddPointObject.txtTitik_X.Text = Convert.ToString(projX);
                formAddPointObject.txtTitik_Y.Text = Convert.ToString(projY);
            }

        }


        //==============================
        //FUNCTION
        //==============================
        public void loadNamaAset()
        {
            //==============================
            //ADD LAYER SARANA IBADAH
            //==============================
            string shpNamaAset = Path.Combine(strFilePath, @"D:\SIG MUKHLISHSemester 2\Prak PGD\acara 6 dan 7\Resources\Spatial\PEMERINTAHAN_PT_50K.shp");
            MapWinGIS.Shapefile sfNamaAset = new MapWinGIS.Shapefile();
            sfNamaAset.Open(shpNamaAset, null);


            MapWinGIS.Image imgkntorcmat = new MapWinGIS.Image();
            imgkntorcmat.Open(Path.Combine(strFilePath, @"D:\SIG MUKHLISHSemester 2\Prak PGD\acara 6 dan 7\Resources\Spatial\KntorCmat.png"),
                ImageType.USE_FILE_EXTENSION, true, null);
            ShapefileCategory ctkntorcmat = sfNamaAset.Categories.Add("kntorcmat");
            ctkntorcmat.Expression = "[Nama] = \"kntorcmat\"";
            ctkntorcmat.DrawingOptions.PointType = tkPointSymbolType.ptSymbolPicture;
            ctkntorcmat.DrawingOptions.Picture = imgkntorcmat;
            ctkntorcmat.DrawingOptions.PictureScaleX = 0.25;
            ctkntorcmat.DrawingOptions.PictureScaleY = 0.25;

            sfNamaAset.DefaultDrawingOptions.Visible = false; //hide all the unclassified points
            sfNamaAset.Categories.ApplyExpressions();
            axMap1.Redraw();

            MapWinGIS.Image imgkntrlrah = new MapWinGIS.Image();
            imgkntrlrah.Open(Path.Combine(strFilePath, @"D:\SIG MUKHLISHSemester 2\Prak PGD\acara 6 dan 7\Resources\Spatial\KntorLrah.png"),
                ImageType.USE_FILE_EXTENSION, true, null);
            ShapefileCategory ctkntrlrah = sfNamaAset.Categories.Add("kntrlrah");
            ctkntrlrah.Expression = "[Nama] = \"kntrlrah\"";
            ctkntrlrah.DrawingOptions.PointType = tkPointSymbolType.ptSymbolPicture;
            ctkntrlrah.DrawingOptions.Picture = imgkntrlrah;
            ctkntrlrah.DrawingOptions.PictureScaleX = 0.25;
            ctkntrlrah.DrawingOptions.PictureScaleY = 0.25;

            sfNamaAset.DefaultDrawingOptions.Visible = false; //hide all the unclassified points
            sfNamaAset.Categories.ApplyExpressions();
            axMap1.Redraw();

            MapWinGIS.Image imgkntrwlkt = new MapWinGIS.Image();
            imgkntrwlkt.Open(Path.Combine(strFilePath, @"D:\SIG MUKHLISHSemester 2\Prak PGD\acara 6 dan 7\Resources\Spatial\KntorWlkt.png"),
                ImageType.USE_FILE_EXTENSION, true, null);
            ShapefileCategory ctkntrwlkt = sfNamaAset.Categories.Add("kntrwlkt");
            ctkntrwlkt.Expression = "[Nama] = \"kntrwlkt\"";
            ctkntrwlkt.DrawingOptions.PointType = tkPointSymbolType.ptSymbolPicture;
            ctkntrwlkt.DrawingOptions.Picture = imgkntrwlkt;
            ctkntrwlkt.DrawingOptions.PictureScaleX = 0.25;
            ctkntrwlkt.DrawingOptions.PictureScaleY = 0.25;

            sfNamaAset.DefaultDrawingOptions.Visible = false; //hide all the unclassified points
            sfNamaAset.Categories.ApplyExpressions();
            axMap1.Redraw();

            //==============================
            //LOAD ATTRIBUTE
            //==============================



            DataGridView1.Rows.Clear();
            KryptonRibbonGroupComboBox_Kecamatan.Items.Clear();

            for (int i = 0; i < sfNamaAset.Table.NumFields; i++)
            {
                DataGridView1.Columns.Add(sfNamaAset.Table.Field[i].Name, sfNamaAset.Table.Field[i].Name);
            }
            DataGridView1.Columns.Add("fid", "fid");

            for (int i = 0; i < sfNamaAset.Table.NumRows; i++)
            {
                string[] myAttributeRow = new string[sfNamaAset.Table.NumFields + 1];
                for (int j = 0; j < sfNamaAset.Table.NumFields; j++)
                {
                    myAttributeRow[j] = sfNamaAset.Table.CellValue[j, i].ToString();
                }
                myAttributeRow[sfNamaAset.Table.NumFields] = i.ToString();
                DataGridView1.Rows.Insert(i, myAttributeRow);

                if (!KryptonRibbonGroupComboBox_Kecamatan.Items.Contains(
                    sfNamaAset.Table.CellValue[sfNamaAset.FieldIndexByName["REMARK"], i].ToString()))
                {
                    KryptonRibbonGroupComboBox_Kecamatan.Items.Add(
                        sfNamaAset.Table.CellValue[sfNamaAset.FieldIndexByName["REMARK"], i].ToString());
                }
            }

            DataGridView1.ClearSelection();
            KryptonRibbonGroupComboBox_Kecamatan.Sorted = true;
        }

        private void Map1_MouseDownEvent(object sender, _DMapEvents_MouseDownEvent e)
        {

        }
    }
}
