/* XML Signal Viewer
 * Andreas Funke
 * This class is the main form code behind. All of the main functionalities are written here.
 */
namespace SignalVisualizer
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using System.Xml.Linq;
    using ZedGraph;

    /// <summary>
    /// Main Form Class.
    /// </summary>
    public partial class MainForm : Form
    {
        #region Private Variables
        /// <summary>
        /// Initialize dictSqWave object.
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<string, string>>> dictSqWave = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        /// <summary>
        /// Initialize dictConstValue object.
        /// </summary>
        private Dictionary<string, double> dictConstValue = new Dictionary<string, double>();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor - MainForm.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Private event based methods

        /// <summary>
        /// Main form loaded event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Common.xmlArgument))
            {
                string xmlfile = Common.xmlArgument;
                XElement rootElement = XElement.Load(xmlfile);
                ShowPointsCheckbox(rootElement);
            }
        }

        /// <summary>
        /// Open submenu click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Filter = "XML Files|*.xml;";
                DialogResult result = openFileDialog1.ShowDialog();
                if (result == DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(openFileDialog1.FileName))
                    {
                        string xmlfile = openFileDialog1.FileName;

                        XElement rootElement = XElement.Load(xmlfile);
                        ShowPointsCheckbox(rootElement);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Exit submenu click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Called when panel is resized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnlSignalViewer_Resize(object sender, EventArgs e)
        {
            foreach (ZedGraphControl ctrls in pnlSignalViewer.Controls)
            {
                ctrls.Size = new System.Drawing.Size(pnlSignalViewer.Width, 200);
            }
        }

        /// <summary>
        /// Called when checkbox is toggled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkbox_CheckStateChanged(object sender, EventArgs e)
        {
            CheckBox chkbox = (CheckBox)sender;
            string name = chkbox.Name.Replace("chk_", "");
            if (chkbox.Checked == false)
            {
                Control[] zgcontrols = pnlSignalViewer.Controls.Find("zedCtrl_" + name, false);
                if (zgcontrols.Length > 0)
                {
                    ZedGraphControl zgcontrol = (ZedGraphControl)zgcontrols[0];
                    zgcontrol.Visible = false;
                }
            }
            else
            {
                Control[] zgcontrols = pnlSignalViewer.Controls.Find("zedCtrl_" + name, false);
                if (zgcontrols.Length > 0)
                {
                    ZedGraphControl zgcontrol = (ZedGraphControl)zgcontrols[0];
                    zgcontrol.Visible = true;
                }
            }
        }

        /// <summary>
        /// Zed graph zoom event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        private void zgControl_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            foreach (ZedGraphControl ctrls in pnlSignalViewer.Controls)
            {
                ZedGraphControl zgcontrol = (ZedGraphControl)ctrls;
                zgcontrol.GraphPane.XAxis.Scale.Min = sender.GraphPane.XAxis.Scale.Min;
                zgcontrol.GraphPane.XAxis.Scale.Max = sender.GraphPane.XAxis.Scale.Max;
                zgcontrol.Refresh();
            }
        }

        /// <summary>
        /// This will trigger when mouse is hovered over plot points.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pane"></param>
        /// <param name="curve"></param>
        /// <param name="iPt"></param>
        /// <returns></returns>
        private string zgControl_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt)
        {
            string remarksVal = string.Empty;
            if (curve.Points[iPt].Tag != null)
                remarksVal = curve.Points[iPt].Tag.ToString();

            return remarksVal;
        }

        #endregion

        #region Private custom methods

        /// <summary>
        /// This method retrieves the points from xml file and plots the points accordingly. This method also creates the zedgraph controls and checkboxes dynamically.
        /// </summary>
        /// <param name="element"></param>
        private void ShowPointsCheckbox(XElement element)
        {
            pnlSignals.Controls.Clear();
            pnlSignalViewer.Controls.Clear();
            dictSqWave = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            dictConstValue = new Dictionary<string, double>();
            foreach (XElement childElement in element.Elements())
            {
                if (childElement.Name.LocalName == "Constants")
                {
                    foreach (XElement innerConstantElement in childElement.Elements())
                    {
                        if (innerConstantElement.Name.LocalName == "Constant")
                        {
                            if (!dictConstValue.ContainsKey(innerConstantElement.Attribute("name").Value))
                                dictConstValue.Add(innerConstantElement.Attribute("name").Value, Convert.ToDouble(innerConstantElement.Attribute("value").Value.Replace("ms", "")));
                        }
                    }
                }
                if (childElement.Name.LocalName == "Signal")
                {
                    CheckBox chkbox = new CheckBox();
                    chkbox.Name = "chk_" + childElement.Attribute("name").Value;
                    chkbox.Text = childElement.Attribute("name").Value;
                    chkbox.Checked = true;
                    chkbox.CheckStateChanged += chkbox_CheckStateChanged;
                    pnlSignals.Controls.Add(chkbox);

                    if (childElement.Attribute("remark") != null)
                    {
                        ToolTip chkBoxToolTip = new System.Windows.Forms.ToolTip();
                        chkBoxToolTip.SetToolTip(chkbox, childElement.Attribute("remark").Value);
                    }

                    ZedGraphControl zgControl = new ZedGraphControl();
                    zgControl.Name = "zedCtrl_" + childElement.Attribute("name").Value;
                    zgControl.Dock = DockStyle.Top;
                    zgControl.Size = new System.Drawing.Size(pnlSignalViewer.Width, 200);
                    zgControl.GraphPane.Title.Text = childElement.Attribute("name").Value;
                    zgControl.GraphPane.Title.FontSpec.Size = 22F;
                    zgControl.GraphPane.XAxis.Scale.MinorStep = 1;
                    zgControl.GraphPane.YAxis.Scale.MajorStep = 1;
                    zgControl.GraphPane.XAxis.Title.Text = "t(s)";
                    zgControl.GraphPane.XAxis.Title.FontSpec.Size = 20F;
                    zgControl.GraphPane.YAxis.Title.Text = childElement.Attribute("name").Value;
                    zgControl.GraphPane.YAxis.Title.FontSpec.Size = 20F;
                    zgControl.IsShowPointValues = true;
                    zgControl.ZoomEvent += zgControl_ZoomEvent;
                    zgControl.PointValueEvent += new ZedGraph.ZedGraphControl.PointValueHandler(zgControl_PointValueEvent);
                    pnlSignalViewer.Controls.Add(zgControl);

                    Dictionary<string, string> dictTimeNTag;
                    Dictionary<string, Dictionary<string, string>> dictTimeLogic = new Dictionary<string, Dictionary<string, string>>();
                    string prevValue = string.Empty;
                    foreach (XElement innerChildElement in childElement.Elements())
                    {
                        if (innerChildElement.Name.LocalName == "SamplePoint")
                        {
                            double timeVal = GetManipulatedtimeValues(innerChildElement.Attribute("time").Value, dictConstValue);
                            string strTimeVal = timeVal.ToString();
                            dictTimeNTag = new Dictionary<string, string>();
                            dictTimeNTag.Add(innerChildElement.Attribute("logic").Value, (innerChildElement.Attribute("remark") != null ? innerChildElement.Attribute("remark").Value : string.Empty));
                            dictTimeLogic.Add(strTimeVal, dictTimeNTag);

                            if (prevValue == "transistion" || prevValue == "transition")
                            {
                                strTimeVal = timeVal.ToString() + "t";
                                dictTimeNTag = new Dictionary<string, string>();
                                dictTimeNTag.Add(innerChildElement.Attribute("logic").Value, (innerChildElement.Attribute("remark") != null ? innerChildElement.Attribute("remark").Value : string.Empty));
                                dictTimeLogic.Add(strTimeVal, dictTimeNTag);
                            }
                            else if (innerChildElement.Attribute("logic").Value == "transistion" || innerChildElement.Attribute("logic").Value == "transition")
                            {
                                strTimeVal = timeVal.ToString() + "t";
                                dictTimeNTag = new Dictionary<string, string>();
                                dictTimeNTag.Add(innerChildElement.Attribute("logic").Value, (innerChildElement.Attribute("remark") != null ? innerChildElement.Attribute("remark").Value : string.Empty));
                                dictTimeLogic.Add(strTimeVal, dictTimeNTag);
                            }
                            prevValue = innerChildElement.Attribute("logic").Value;
                        }
                    }
                    dictSqWave.Add(childElement.Attribute("name").Value, dictTimeLogic);
                }
            }

            PlotPoints(dictSqWave);
        }

        /// <summary>
        /// This method gets the final time value - after addition with transition and constants.
        /// </summary>
        /// <param name="timeVal"></param>
        /// <param name="dictConstValue"></param>
        /// <returns></returns>
        private double GetManipulatedtimeValues(string timeVal, Dictionary<string, double> dictConstValue)
        {
            double manipulatedTimeVal = 0.0;
            if (dictConstValue.Count > 0)
            {
                string combinedString = string.Empty;
                for (int i = 0; i < dictConstValue.Count; i++)
                {
                    if (timeVal.Contains(dictConstValue.Keys.ElementAt(i)))
                        combinedString = timeVal.Replace("ms", "").Replace(dictConstValue.Keys.ElementAt(i), dictConstValue[dictConstValue.Keys.ElementAt(i)].ToString());

                    double manipulatedVal = 0.0;
                    if (combinedString.Contains("+"))
                    {

                        string[] strArray = combinedString.Split('+');
                        foreach (var item in strArray)
                        {
                            manipulatedVal += Convert.ToDouble(item);
                        }
                    }
                    if (combinedString.Contains("-"))
                    {

                        string[] strArray = combinedString.Split('-');
                        foreach (var item in strArray)
                        {
                            manipulatedVal += Convert.ToDouble(item);
                        }
                    }

                    manipulatedTimeVal = manipulatedVal;
                }

                if (string.IsNullOrEmpty(combinedString))
                {
                    manipulatedTimeVal = Convert.ToDouble(timeVal.Replace("ms", ""));
                }
            }
            else
            {
                manipulatedTimeVal = Convert.ToDouble(timeVal.Replace("ms", ""), System.Globalization.CultureInfo.InvariantCulture);
            }

            return manipulatedTimeVal;
        }

        /// <summary>
        /// This method plots the points onto graph.
        /// </summary>
        /// <param name="dictSqWave"></param>
        private void PlotPoints(Dictionary<string, Dictionary<string, Dictionary<string, string>>> dictSqWave)
        {
            if (dictSqWave.Count > 0)
            {
                foreach (var outerItem in dictSqWave)
                {
                    string signalName = outerItem.Key;
                    Control[] zgcontrols = pnlSignalViewer.Controls.Find("zedCtrl_" + signalName, false);
                    if (zgcontrols.Length > 0)
                    {
                        ZedGraphControl zgcontrol = (ZedGraphControl)zgcontrols[0];

                        foreach (var innerItem in outerItem.Value)
                        {
                            bool isAvbl = CheckIsTransitionPresent(innerItem.Value);

                            if (isAvbl)
                            {
                                string prevType = string.Empty;
                                int i = 0;
                                IEnumerable<PlotsAndType> plots = CustomPlotLines(outerItem.Value);
                                List<double> xList = new List<double>();
                                List<double> yList = new List<double>();
                                bool CloseNext = false;
                                foreach (var itm in plots)
                                {
                                    xList.Add(itm.x);
                                    yList.Add(itm.y);
                                    i++;

                                    if (CloseNext)
                                    {
                                        i = 0; //resetting to zero
                                        CloseNext = false;
                                        LineItem line = new LineItem(itm.Tag, xList.ToArray(), yList.ToArray(), Color.Purple, SymbolType.None);
                                        zgcontrol.GraphPane.Legend.IsVisible = false;
                                        zgcontrol.GraphPane.CurveList.Add(line);

                                        line.Line.Fill = new Fill(Color.Pink);
                                        line.Line.StepType = StepType.ForwardStep;
                                        xList.Clear();
                                        yList.Clear();
                                        continue;
                                    }

                                    if (itm.type == "transistion" && i != 1)
                                    {
                                        i = 0; //resetting to zero
                                        LineItem line = new LineItem(itm.Tag, xList.ToArray(), yList.ToArray(), Color.Purple, SymbolType.None);
                                        zgcontrol.GraphPane.Legend.IsVisible = false;
                                        zgcontrol.GraphPane.CurveList.Add(line);

                                        line.Line.StepType = StepType.ForwardStep;
                                        xList.Clear();
                                        yList.Clear();
                                        continue;
                                    }
                                    else if (itm.type == "transistion" && i == 1)
                                    {
                                        CloseNext = true;
                                    }
                                }
                            }
                            else
                            {
                                PointPairList list = new PointPairList();
                                foreach (var t in SquareWave(outerItem.Value).Take(20))
                                    list.Add(t.Item1, t.Item2.yVal, t.Item2.Tag);
                                LineItem myCurve = zgcontrol.GraphPane.AddCurve("Plot Line", list, Color.Blue, SymbolType.None);
                                myCurve.Line.StepType = StepType.ForwardStep;
                                zgcontrol.GraphPane.Legend.IsVisible = false;
                            }
                        }
                        //zgcontrol.AxisChange();
                    }
                }
            }
        }

        /// <summary>
        /// This method is for transition based signal points.
        /// </summary>
        /// <param name="dictPlotLines"></param>
        /// <returns></returns>
        IEnumerable<PlotsAndType> CustomPlotLines(Dictionary<string, Dictionary<string, string>> dictPlotLines)
        {
            string prevValue = string.Empty;
            double prevYVal = 0.0;
            foreach (var item in dictPlotLines)
            {
                double y = (item.Value.First().Key == "true" ? 1 : 0);
                if (item.Value.First().Key == "transistion" || item.Value.First().Key == "transition")
                {
                    y = 0.0;
                }
                if (prevValue == "transistion" || prevValue == "transition")
                {
                    if (item.Value.First().Key == "true")
                        y = 1.0;
                    if (prevYVal == 1.0)
                        y = 0.0;
                    else if (prevYVal == 0.0)
                        y = 1.0;
                }

                prevYVal = y;
                prevValue = item.Value.First().Key;

                double xVal = Convert.ToDouble(item.Key.Replace("t", ""));
                double xConvertedVal = ConvertMillisecondsToSeconds(xVal);

                PlotsAndType plots = new PlotsAndType();
                plots.x = xConvertedVal;
                plots.y = y;
                plots.type = item.Value.First().Key;
                plots.Tag = (!string.IsNullOrEmpty(item.Value.First().Value) ? item.Value.First().Value : string.Empty);

                yield return plots;
            }
        }

        /// <summary>
        /// This method checks whether the xml node has transition value.
        /// </summary>
        /// <param name="dictSqWave"></param>
        /// <returns></returns>
        private bool CheckIsTransitionPresent(Dictionary<string, string> dictSqWave)
        {
            bool isAvbl = false;
            foreach (var item in dictSqWave)
            {
                if (item.Key.ToLower().Contains("transition"))
                {
                    isAvbl = true;
                    break;
                }
                if (item.Key.ToLower().Contains("transistion"))
                {
                    isAvbl = true;
                    break;
                }
            }
            return isAvbl;
        }

        /// <summary>
        /// This method returns square tuple based high and low value based on signal points.
        /// </summary>
        /// <param name="sqWave"></param>
        /// <returns></returns>
        IEnumerable<Tuple<double, PlotNTag>> SquareWave(Dictionary<string, Dictionary<string, string>> sqWave)
        {
            string prevValue = string.Empty;
            double prevYVal = 0.0;
            foreach (var item in sqWave)
            {
                double y = (item.Value.First().Key == "true" ? 1 : 0);
                if (item.Value.First().Key == "transistion" || item.Value.First().Key == "transition")
                {
                    y = 0.0;
                }
                if (prevValue == "transistion" || prevValue == "transition")
                {
                    if (item.Value.First().Key == "true")
                        y = 1.0;
                    if (prevYVal == 1.0)
                        y = 0.0;
                    else if (prevYVal == 0.0)
                        y = 1.0;
                }

                prevYVal = y;
                prevValue = item.Value.First().Key;

                double xVal = Convert.ToDouble(item.Key.Replace("t", ""));
                double xConvertedVal = ConvertMillisecondsToSeconds(xVal);
                PlotNTag plot = new PlotNTag();
                plot.yVal = y;
                plot.Tag = (!string.IsNullOrEmpty(item.Value.First().Value) ? item.Value.First().Value : string.Empty);

                yield return Tuple.Create(xConvertedVal, plot);
            }
        }

        /// <summary>
        /// This method returns converted seconds value when milliseconds is provided.
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static double ConvertMillisecondsToSeconds(double milliseconds)
        {
            return Convert.ToDouble((milliseconds / 1000));//TimeSpan.FromMilliseconds(milliseconds).TotalSeconds;
        }

        #endregion
    }

    /// <summary>
    /// Class for plot points and type.
    /// </summary>
    public class PlotsAndType
    {
        public double x = 0.0;
        public double y = 0.0;
        public string type = string.Empty;
        public string Tag = string.Empty;
    }

    public class PlotNTag
    {
        public double yVal = 0.0;
        public string Tag = string.Empty;
    }
}
