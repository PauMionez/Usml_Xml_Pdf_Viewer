using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Utils;
using Syncfusion.Windows.PdfViewer;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using Usml_Xml_Pdf_Viewer.Model;
using Usml_Xml_Pdf_Viewer.Service;

namespace Usml_Xml_Pdf_Viewer.ViewModel
{
    internal class MainViewModel : Abstract.ViewModelBase
    {
        public AsyncCommand<DocumentViewer> SelectPDFCommand { get; private set; }
        public AsyncCommand<TextEditor> AvalonTextEditor_LoadedCommand { get; private set; }

        public DevExpress.Mvvm.DelegateCommand<PdfViewerControl> CurrentPageChanged { get; private set; }

        public DevExpress.Mvvm.DelegateCommand XMLViewerMouseHoverCommand {  get; private set; }
        public DevExpress.Mvvm.DelegateCommand PDFViewerMouseHoverCommand {  get; private set; }

        public MainViewModel()
        {
            SelectPDFCommand = new AsyncCommand<DocumentViewer>(SelectPDFFile);
            AvalonTextEditor_LoadedCommand = new AsyncCommand<TextEditor>(AvalonTextEditor_Loaded);

            CurrentPageChanged = new DevExpress.Mvvm.DelegateCommand<PdfViewerControl>(PDFPageScroll);

            XMLViewerMouseHoverCommand = new DevExpress.Mvvm.DelegateCommand(OnXMLViewerMouseHover);
            PDFViewerMouseHoverCommand = new DevExpress.Mvvm.DelegateCommand(OnPDFViewerMouseHover);

            DocumentCollection = new ObservableCollection<DocumentModel>();
            PdfDocumentCollection = new ObservableCollection<DocumentModel>();

            XmlTagsCollection = new ObservableCollection<xmlTagModel>();
            LoadingStatusPdf = Visibility.Hidden;


        }


        private DocumentModel _SelectedPdfDocumentItem;

        public DocumentModel SelectedPdfDocumentItem
        {
            get { return _SelectedPdfDocumentItem; }
            set { _SelectedPdfDocumentItem = value; OnPropertyChanged(); }
        }

        private ObservableCollection<DocumentModel> _PdfDocumentCollection;

        public ObservableCollection<DocumentModel> PdfDocumentCollection
        {
            get { return _PdfDocumentCollection; }
            set { _PdfDocumentCollection = value; OnPropertyChanged(); }
        }

        private ObservableCollection<DocumentModel> _AllPosiblePages;

        public ObservableCollection<DocumentModel> AllPosiblePages
        {
            get { return _AllPosiblePages; }
            set { _AllPosiblePages = value; OnPropertyChanged(); }
        }

        private int _SelectedPdfDocumentIndex;

        public int SelectedPdfDocumentIndex
        {
            get { return _SelectedPdfDocumentIndex; }
            set { _SelectedPdfDocumentIndex = value; OnPropertyChanged(); }
        }

        private string _SelectedPDFSourceStream;

        public string SelectedPDFSourceStream
        {
            get { return _SelectedPDFSourceStream; }
            set { _SelectedPDFSourceStream = value; OnPropertyChanged(); }
        }

        private Visibility _loadingStatusPdf;

        public Visibility LoadingStatusPdf
        {
            get { return _loadingStatusPdf; }
            set { _loadingStatusPdf = value; OnPropertyChanged(); }
        }


        private TextDocument _CodingTxtDocument;
        public TextDocument CodingTxtDocument
        {
            get { return _CodingTxtDocument; }
            set { _CodingTxtDocument = value; OnPropertyChanged(); }
        }

        private DocumentModel _SelectedDocumentItem;
        public DocumentModel SelectedDocumentItem
        {
            get { return _SelectedDocumentItem; }
            set { _SelectedDocumentItem = value; OnPropertyChanged(); }
        }

        private int _currentPDFPage;

        public int CurrentPDFPage
        {
            get { return _currentPDFPage; }
            set { _currentPDFPage = value; OnPropertyChanged(); }
        }

        private ObservableCollection<DocumentModel> _DocumentCollection;

        public ObservableCollection<DocumentModel> DocumentCollection
        {
            get { return _DocumentCollection; }
            set { _DocumentCollection = value; OnPropertyChanged(); }
        }

        private ObservableCollection<xmlTagModel> _xmlTagsCollection;

        public ObservableCollection<xmlTagModel> XmlTagsCollection
        {
            get { return _xmlTagsCollection; }
            set { _xmlTagsCollection = value; OnPropertyChanged(); }
        }

        private CurrentPageChangedEventHandler _pdfCurrenpageChanged;

        public CurrentPageChangedEventHandler PdfCurrenpageChanged
        {
            get { return _pdfCurrenpageChanged; }
            set { _pdfCurrenpageChanged = value; OnPropertyChanged(); }
        }


        private bool _isCheckboxXMLscroll;

        public bool IsCheckboxXMLscroll
        {
            get { return _isCheckboxXMLscroll; }
            set
            {
                if (_isCheckboxXMLscroll != value)
                {
                    _isCheckboxXMLscroll = value;
                    if (_isCheckboxXMLscroll) 
                    {
                        IsCheckboxPDFscroll = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        private bool _isCheckboxPDFscroll;

        public bool IsCheckboxPDFscroll
        {
            get { return _isCheckboxPDFscroll; }
            set
            {
                if (_isCheckboxPDFscroll != value)
                {
                    _isCheckboxPDFscroll = value;
                    if (_isCheckboxPDFscroll)
                    {
                        IsCheckboxXMLscroll = false;
                    }
                    OnPropertyChanged();
                }
            }
        }



        public DocumentViewer documentViewerInteropControl = null;
        public TextEditor CodingTextControl = null;
        //public string GlobalPDFFilePath;
        public bool UpperPage;
        public bool BottomPage;
        public bool BothTopBottom;
        public string lastDetectedPage;




        /// <summary>
        /// Load the document into the control. Set the control as a global variable.
        /// </summary>
        /// <param name="documentViewerXamlControl"></param>
        /// <returns></returns>
        private async Task SelectPDFFile(DocumentViewer documentViewerXamlControl)
        {
            try
            {
                // Set the control global variable
                documentViewerInteropControl = documentViewerXamlControl;
                CodingTxtDocument = new TextDocument();


                //Get file path
                string xmlFilePath = GetFilePath(@"Select docx files (*.xml)", "*.xml", "Open multiple xml documents");
                if (xmlFilePath == null) return;

                string pdfFilePath = GetFilePath(@"Select pdf files (*.pdf)", "*.pdf", "Open multiple pdf documents");
                if (pdfFilePath == null) return;


                string textContent = "";

                // Load xml file in textviewer
                using (FileStream fs = new FileStream(xmlFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    using (StreamReader reader = FileReader.OpenStream(fs, Encoding.UTF8))
                    {
                        textContent = await reader.ReadToEndAsync();
                        CodingTxtDocument = new TextDocument(textContent);
                    }
                }


                await Task.Run(async () =>
                {
                    //Read and Get pdf contents and save in collection
                    await GetPDFTopBottomContent(pdfFilePath);

                    //Get all page tags and save in collection
                    await ExtractPageTags(textContent);
                });


                await Task.Run(() =>
                {
                    SelectedPDFSourceStream = pdfFilePath;
                });


                //// Get the directory and filename (without extension) of the input PDF
                //string inputDirectory = Path.GetDirectoryName(pdfFilePath);
                //string inputFileNameWithoutExt = Path.GetFileNameWithoutExtension(pdfFilePath);
                //string outputFilePath = Path.Combine(inputDirectory, $"{inputFileNameWithoutExt}_output.txt");

                //await WriteOutputToTextFile(outputFilePath);


            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }


        private async Task AvalonTextEditor_Loaded(TextEditor xamlInterfaceControlElement)
        {
            try
            {
                CodingTextControl = xamlInterfaceControlElement;
                //await Task.Run(() =>
                //{
                if (CodingTextControl?.TextArea?.TextView != null)
                {
                    CodingTextControl.TextArea.TextView.ScrollOffsetChanged += TextViewScrollOffsetChanged;
                }
                // });
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private async void TextViewScrollOffsetChanged(object sender, EventArgs e)
        {
            try
            {
                // Check if XML Scroll is enabled
                if (!IsCheckboxXMLscroll) { return; }

                GetPDFContent GetPDFContentService = new GetPDFContent();

                var textView = sender as ICSharpCode.AvalonEdit.Rendering.TextView;
                if (textView == null || CodingTxtDocument == null)
                { return; }

                // Get the visible lines from the viewer
                List<int> visibleLines = textView.VisualLines.Select(vl => vl.FirstDocumentLine.LineNumber).ToList();


                if (visibleLines.Count == 0)
                { return; }

                // Get the visible text
                int firstVisibleLine = visibleLines.First();
                int lastVisibleLine = visibleLines.Last();

                // Construct the visible text from the document lines between the first and last visible line
                string visibleText = string.Join(
                       Environment.NewLine,
                       CodingTxtDocument.Lines
                        .Skip(firstVisibleLine - 1)// Adjusting for 0-based index
                        .Take(lastVisibleLine - firstVisibleLine + 1) //Center view // Taking lines within the visible range
                        .Select(line => CodingTxtDocument.GetText(line)) // Get the text of each line
                );

                // Extract the page number from the visible text
                string pageText = TagPageRegex(visibleText);


                //// Find pagetext in pdf 
                if (pageText != null && lastDetectedPage != pageText)
                {
                    lastDetectedPage = pageText;
                    int getPdfPage = GetPDFContentService.FindPDFPageByContent(DocumentCollection, pageText, UpperPage, BottomPage, BothTopBottom);

                    if (getPdfPage != 0)
                    {
                        CurrentPDFPage = getPdfPage;

                    }
                    else { return; }
                }

                #region comment code

                ////// Find pagetext in pdf 
                //if (pageText != null && lastDetectedPage != pageText)
                //{
                //    lastDetectedPage = pageText;
                //    await FindPDFPageByContent(GlobalPDFFilePath, pageText, UpperPage, BottomPage);
                //    //WarningMessage($"the text {pageText}");
                //}



                //if (LastPageNumber != CurrentPDFPage && pageText != null && pageText != lastDetectedPage)
                //{

                //    if (LastPageNumber != CurrentPDFPage)
                //    {


                //        LastPageNumber = await FindPDFPageByContent(GlobalPDFFilePath, pageText, UpperPage, BottomPage);
                //        lastDetectedPage = pageText;

                //    }
                //}


                // Update CurrentPDFPage if a page tag is found and is different
                // Now find the corresponding page in the PDF

                //if (int.TryParse(pageText, out int pageNumbers))
                //{
                //    int? pageNumber = pageNumbers;

                //    if (pageNumber.HasValue && pageNumber.Value != CurrentPDFPage)
                //    {
                //        if (lastDetectedPage != pageNumber.Value)
                //        {
                //            lastDetectedPage = pageNumber.Value;
                //            await FindPDFPageByContent(GlobalPDFFilePath, pageNumber.Value, UpperPage, BottomPage);
                //        }
                //    }
                //}
                //else
                //{
                //    await FindPDFPageByContent(GlobalPDFFilePath, pageNumber.Value, UpperPage, BottomPage);
                //}
                #endregion

            }
            catch (Exception ex)
            {
                ErrorMessage(ex);

            }

        }

        /// <summary>
        /// PDF Page Scroll find the tags in xml text editor
        /// </summary>
        /// <param name="pdfViewer"></param>
        private void PDFPageScroll(PdfViewerControl pdfViewer)
        {
            // Check if PDF Scroll is enabled
            if (!IsCheckboxPDFscroll) { return; }


            if (pdfViewer != null)
            {
                int newPDFPage = pdfViewer.CurrentPageIndex;

                foreach (DocumentModel page in DocumentCollection)
                {
                    if (page.pdfPage == newPDFPage)
                    {
                        foreach (xmlTagModel tags in XmlTagsCollection)
                        {
                            if (page.TopContent.IndexOf(tags.PageOnly, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                int index = CodingTextControl.Text.IndexOf(tags.FormatTag, StringComparison.OrdinalIgnoreCase);

                                if (index >= 0)
                                {
                                    CodingTextControl.ScrollToLine(CodingTextControl.Document.GetLineByOffset(index).LineNumber);
                                    CodingTextControl.Select(index, tags.FormatTag.Length);
                                    CodingTextControl.Focus();

                                    return;
                                }

                            }
                        }

                    }

                }
            }

        }


        private string TagPageRegex(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return null;

                var pageonly = new Regex(@"<page>\s*([IVXLCDM\d]+)\s*</page>", RegexOptions.IgnoreCase);
                var pageIdentifier = new Regex(@"<page\s+identifier=""([^""]+)""\s*>\s*(\d+)\s*</page>", RegexOptions.IgnoreCase);
                var pageIdentifierPosition = new Regex(@"<page\s+identifier=""([^""]+)""\s+renderingPosition=""([^""]+)"">\s*(\d+)\s*</page>", RegexOptions.IgnoreCase);


                if (pageIdentifierPosition.IsMatch(text))
                {
                    foreach (Match match in pageIdentifierPosition.Matches(text))
                    {
                        string identifier = match.Groups[1].Value;
                        string renderingPosition = match.Groups[2].Value;
                        string pagevalue = match.Groups[3].Value;

                        if (renderingPosition == "bottom")
                        {
                            BottomPage = true;
                            UpperPage = false;
                        }
                        else
                        {
                            BottomPage = false;
                        }

                        return pagevalue;
                        //Console.WriteLine($"Identifier: {identifier}, Position: {renderingPosition}, Page: {pageNumber}");
                    }
                }
                else if (pageIdentifier.IsMatch(text))
                {
                    foreach (Match match in pageIdentifier.Matches(text))
                    {
                        string identifier = match.Groups[1].Value;
                        string pagevalue = match.Groups[2].Value;

                        BottomPage = false;
                        UpperPage = true;

                        return pagevalue;
                    }
                }
                else if (pageonly.IsMatch(text))
                {
                    foreach (Match match in pageonly.Matches(text))
                    {
                        string pagevalue = match.Groups[1].Value.Trim();

                        BothTopBottom = true;
                        UpperPage = false;
                        BottomPage = false;

                        return pagevalue;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
                return null;
            }
        }


        private async Task GetPDFTopBottomContent(string pdfFile)
        {
            try
            {
                GetPDFContent GetPDFContentService = new GetPDFContent();

                using (PdfDocument document = PdfDocument.Open(pdfFile))
                {
                    // Loop through all pages in the PDF document
                    for (int i = 1; i <= document.NumberOfPages; i++)
                    {
                        // Get the text content of the page
                        var pageText = document.GetPage(i);

                        // Extract words from the page
                        var words = pageText.GetWords();

                        // Use RecursiveXYCut to analyze layout
                        var recursiveXYCut = new RecursiveXYCut(new RecursiveXYCut.RecursiveXYCutOptions()
                        {
                            MinimumWidth = pageText.Width / 3.0,  // Split page width into thirds
                            DominantFontWidthFunc = letters => letters.Select(l => l.GlyphRectangle.Width).Average(),
                            DominantFontHeightFunc = letters => letters.Select(l => l.GlyphRectangle.Height).Average()
                        });

                        var blocks = recursiveXYCut.GetBlocks(words);

                        string toptext = GetPDFContentService.TopLeftRightText(blocks, pageText);
                        string bottomtext = GetPDFContentService.BottomLeftRightText(blocks, pageText);

                        // Save to collection
                        DocumentCollection.Add(new DocumentModel
                        {
                            TopContent = toptext,
                            BottomContent = bottomtext,
                            pdfPage = i
                        });


                        #region trash code


                        // If 'checkUpperOnly' is true, only process top blocks
                        //if (checkUpperOnly)
                        //{
                        //    //var topBlocks = blocks.OrderBy(b => b.BoundingBox.Top).Take(1).ToList();
                        //    //var topBlocks = blocks.Where(b => b.BoundingBox.Bottom > (pageText.Height * 2 / 3)).ToList();
                        //    //var topBlocks = blocks.OrderByDescending(b => b.BoundingBox.Top).Take(1).ToList();
                        //    //string topText = string.Join(" ", topBlocks.Select(b => b.Text));

                        //    string toptext = TopLeftRightText(blocks, pageText);

                        //    if (toptext.Contains(pageNumber))
                        //    {
                        //        //WarningMessage($"Found page number {pageNumber} in the top content of PDF page {i}");
                        //        CurrentPDFPage = i;
                        //        break;
                        //    }
                        //}
                        //else if (checkBottomOnly)
                        //{

                        //    // Filter blocks that are in the bottom third of the page
                        //    //var bottomBlocks = blocks.Where(b => b.BoundingBox.Top < (pageText.Height / 3)).ToList();
                        //    //var bottomBlocks = blocks.OrderByDescending(b => b.BoundingBox.Top).Take(3).ToList();
                        //    //var bottomBlocks = blocks.OrderBy(b => b.BoundingBox.Bottom).Take(1).ToList();
                        //    //string bottomText = string.Join(" ", bottomBlocks.Select(b => b.Text));

                        //    string bottomtext = BottomLeftRightText(blocks, pageText);

                        //    if (bottomtext.Contains(pageNumber))
                        //    {
                        //        //WarningMessage($"Found page number {pageNumber} in the bottom content of PDF page {i}");
                        //        CurrentPDFPage = i;
                        //        break;
                        //    }

                        //}
                        //else
                        //{
                        //    //var fulltopBlocks = blocks.Where(b => b.BoundingBox.Bottom > (pageText.Height * 2 / 3)).ToList();
                        //    //string fulltopText = string.Join(" ", fulltopBlocks.Select(b => b.Text));

                        //    //var fullbottomBlocks = blocks.OrderBy(b => b.BoundingBox).Take(1).ToList();
                        //    //string fullbottomText = string.Join(" ", fulltopBlocks.Select(b => b.Text));
                        //    string toptext = TopLeftRightText(blocks, pageText);
                        //    string bottomtext = BottomLeftRightText(blocks, pageText);


                        //    string allText = string.Join(" ", toptext, bottomtext);

                        //    // Roman numbers
                        //    //string fullPageText = pageText.Text;

                        //    if (allText.Contains(pageNumber))
                        //    {
                        //        //WarningMessage($"Found page number {pageNumber} in PDF page {i}");
                        //        CurrentPDFPage = i;
                        //        break;
                        //    }
                        //}
                        #endregion

                    }
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(ex);

            }
        }

        private async Task ExtractPageTags(string xmlContent)
        {
            try
            {
                // Regular expressions for different tag types
                //var pageOnlyRegex = new Regex(@"<page>\s*([IVXLCDM\d]+)\s*</page>", RegexOptions.IgnoreCase);
                var pageIdentifierRegex = new Regex(@"<page\s+identifier=""([^""]+)""\s*>\s*([IVXLCDM\d]+)\s*</page>", RegexOptions.IgnoreCase);
                var pageIdentifierPositionRegex = new Regex(@"<page\s+identifier=""([^""]+)""\s+renderingPosition=""([^""]+)"">\s*([IVXLCDM\d]+)\s*</page>", RegexOptions.IgnoreCase);


                // Match and process <page> tags
                //foreach (Match match in pageOnlyRegex.Matches(xmlContent))
                //{
                //    XmlTagsCollection.Add(new xmlTagModel
                //    {
                //        FormatTag = match.Value,
                //        PageOnly = match.Groups[1].Value,
                //        PageIdentifier = null,
                //        PageRenderingPosition = null
                //    });
                //}

                // Match and process <page identifier="..." > tags
                foreach (Match match in pageIdentifierRegex.Matches(xmlContent))
                {
                    XmlTagsCollection.Add(new xmlTagModel
                    {
                        FormatTag = match.Value,
                        PageOnly = match.Groups[2].Value,
                        PageIdentifier = match.Groups[1].Value,
                        PageRenderingPosition = null
                    });
                }

                // Match and process <page identifier="..." renderingPosition="..." > tags
                foreach (Match match in pageIdentifierPositionRegex.Matches(xmlContent))
                {
                    XmlTagsCollection.Add(new xmlTagModel
                    {
                        FormatTag = match.Value,
                        PageOnly = match.Groups[3].Value,
                        PageIdentifier = match.Groups[1].Value,
                        PageRenderingPosition = match.Groups[2].Value
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }

        }


        private async Task WriteOutputToTextFile(string outputFilePath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                foreach (DocumentModel document in DocumentCollection)
                {
                    sb.AppendLine($"{document.TopContent} - {document.BottomContent} - Page {document.pdfPage}");
                }

                File.WriteAllText(outputFilePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }

        private async Task WriteOutputToTextFileXml(string outputFilePath)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                foreach (xmlTagModel document in XmlTagsCollection)
                {
                    sb.AppendLine($"{document.PageOnly} - {document.PageIdentifier} - {document.PageRenderingPosition}");
                }

                File.WriteAllText(outputFilePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ErrorMessage(ex);
            }
        }


        private void OnXMLViewerMouseHover()
        {
            //WarningMessage("Mouse Hover is in Xml text editor");
            IsCheckboxXMLscroll = true;
        }

        private void OnPDFViewerMouseHover()
        {
            //WarningMessage("Mouse Hover is in PDF viewer");
            IsCheckboxPDFscroll = true;
        }






        //private void PdfViewerScrollChanged(ScrollChangedEventArgs arg)
        //{
        //    try
        //    {
        //        if (CodingTextControl == null || documentViewerInteropControl == null)
        //            return;

        //        // Calculate scroll percentage from PDF viewer
        //        double pdfScrollRatio = documentViewerInteropControl.VerticalOffset /
        //                                (documentViewerInteropControl.ExtentHeight - documentViewerInteropControl.ViewportHeight);

        //        // Calculate equivalent scroll position in the text editor
        //        double textEditorScrollPosition = pdfScrollRatio * (CodingTextControl.ExtentHeight - CodingTextControl.ViewportHeight);

        //        // Apply scroll position to text editor
        //        CodingTextControl.ScrollToVerticalOffset(textEditorScrollPosition);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMessage(ex);
        //    }
        //}


        #region trash code
        //public string BottomLeftRightText(IReadOnlyList<UglyToad.PdfPig.DocumentLayoutAnalysis.TextBlock> blocks, UglyToad.PdfPig.Content.Page pageText)
        //{
        //    // Get top-left and top-right blocks (only the first line)
        //    var bottomLeftBlock = blocks
        //        .Where(b => b.BoundingBox.Bottom > (pageText.Height * 2 / 3) && b.BoundingBox.Right <= (pageText.Width / 2))
        //        .OrderByDescending(b => b.BoundingBox.Top)
        //        .Take(1).ToList();

        //    var bottomRightBlock = blocks
        //        .Where(b => b.BoundingBox.Bottom > (pageText.Height * 2 / 3) && b.BoundingBox.Left > (pageText.Width / 2))
        //        .OrderByDescending(b => b.BoundingBox.Top)
        //        .Take(1).ToList();

        //    string bottomLeftRight = string.Join(" ", bottomLeftBlock, bottomRightBlock);

        //    return $"{bottomLeftRight}".Trim();

        // Get bottom-left blocks (first two lines from the bottom left quarter)
        //var bottomLeftBlocks = blocks
        //    .Where(b => b.BoundingBox.TopRight.Y < (pageText.Height / 3) && b.BoundingBox.TopRight.X <= (pageText.Width / 2))
        //    .OrderBy(b => b.BoundingBox.TopRight.Y)
        //    .Take(1)  // Take first two lines
        //    .Select(b => b.Text);

        //// Get bottom-right blocks (first two lines from the bottom right quarter)
        //var bottomRightBlocks = blocks
        //    .Where(b => b.BoundingBox.TopRight.Y < (pageText.Height / 3) && b.BoundingBox.TopRight.X > (pageText.Width / 2))
        //    .OrderBy(b => b.BoundingBox.TopRight.Y)
        //    .Take(1)  // Take first two lines
        //    .Select(b => b.Text);

        //string bottomLeftText = string.Join(" ", bottomLeftBlocks);
        //string bottomRightText = string.Join(" ", bottomRightBlocks);

        //return $"{bottomLeftText} {bottomRightText}".Trim();


        //}


        //public string TopLeftRightText(IReadOnlyList<UglyToad.PdfPig.DocumentLayoutAnalysis.TextBlock> blocks, UglyToad.PdfPig.Content.Page pageText)
        //{
        //    // Get bottom-left and bottom-right blocks (only the first line)
        //    var topLeftBlock = blocks
        //        .Where(b => b.BoundingBox.Top < (pageText.Height / 3) && b.BoundingBox.Right <= (pageText.Width / 2))
        //        .OrderBy(b => b.BoundingBox.Top)
        //        .Take(1).ToList();

        //    var topRightBlock = blocks
        //        .Where(b => b.BoundingBox.Top < (pageText.Height / 3) && b.BoundingBox.Left > (pageText.Width / 2))
        //        .OrderBy(b => b.BoundingBox.Top)
        //        .Take(1).ToList();
        //    //.FirstOrDefault();

        //    string topLeftRight = string.Join(" ", topLeftBlock, topRightBlock);

        //    return $"{topLeftRight}".Trim();

        // Get top-left blocks 
        //var topLeftBlocks = blocks
        //    .Where(b => b.BoundingBox.TopLeft.Y > (pageText.Height * 2 / 3) && b.BoundingBox.TopLeft.X <= (pageText.Width / 2))
        //    .OrderByDescending(b => b.BoundingBox.TopLeft.Y)
        //    .Take(1)  // Take first two lines
        //    .Select(b => b.Text);

        //// Get top-right blocks 
        //var topRightBlocks = blocks
        //    .Where(b => b.BoundingBox.TopRight.Y > (pageText.Height * 2 / 3) && b.BoundingBox.TopRight.X > (pageText.Width / 2))
        //    .OrderByDescending(b => b.BoundingBox.TopRight.Y)
        //    .Take(1)  // Take first two lines
        //    .Select(b => b.Text);


        //string topLeftText = string.Join(" ", topLeftBlocks);
        //string topRightText = string.Join(" ", topRightBlocks);
        //return $"{topLeftText} {topRightText}".Trim();


        //}


        //private async Task getPageFromPDF(string pdfFile, string pageToFind)
        //{
        //    //List<string> pagesInsidePDF = new List<string>();
        //    List<int> matchingPages = new List<int>();
        //    using (UglyToad.PdfPig.PdfDocument document = UglyToad.PdfPig.PdfDocument.Open(pdfFile))
        //    {
        //        //PdfDocumentBuilder builder = new PdfDocumentBuilder();

        //        // Loop through all pages in the input PDF
        //        for (int pageNumber = 1; pageNumber <= document.NumberOfPages; pageNumber++)
        //        {
        //            UglyToad.PdfPig.Content.Page page = document.GetPage(pageNumber);
        //            //builder.AddPage(page); // Add the current page to the new document
        //            string pageText = page.Text;

        //            if (pageText.Contains(pageToFind))
        //            {
        //                matchingPages.Add(pageNumber); // Store the page number that contains "203"
        //            }

        //            //pagesInsidePDF.Add(pageText);
        //        }
        //    }

        //}
        #endregion
    }
}
