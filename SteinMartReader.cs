using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.IO;
// using System.Windows.Forms;

namespace FastLoad
{
    public enum dicFmt
    {
        TransControlNo,
        POType,
        PONumber,
        PODate,
        DepartmentNo,
        VendorNo,
        BuyerNo,
        ShipDate,
        CancelAfter,
        WarehouseDescription,
        WarehouseNumber,
        Qty,
        QtyUnit,
        UnitPrice,
        BasisofUnitPrice,
        SKUNumber,
        UP_VA_UKCode1,
        UP_VA_UKNo1,
        UP_VA_UKCode2,
        UP_VA_UKNo2,
        ExtendedLineAmountt,
        StoreNoNotThere,
        StoreQuantity,
        TotalPOAmountt,
        NoofLineItems,
        TotalOrderQuantityy,
        ShipToStoreName,
        ShipToStoreAddress1,
        ShipToStoreAddress2,
        ShipToStoreCity,
        ShipToStoreState,
        ShipToStoreZipcode,
        ShipToLocation,
        MarkForLocation,
        SKU_UPNumber,
        CrossVendorItem,
        CrossColor,
        CrossSize,
        CrossDescr,
        SalesRequirementCode1,
        SalesRequirementCode2,
        PromotionStart,
        SpecialOrderType,
        TermsDescription,
        Instructions,
        Message,
        Routing,
        BuyerContactName,
        BuyerContactPhone,
        DeliveryContactName,
        DeliveryContactPhone,
        RetailPrice,
        Hanger_PackingTypeCode,
        Hanger_PackingTypeDescr,
        BlanketOrderNo,
        Priority,
        VendorStyle,
        TransType,
        InstructionsID,
        ManufacturersSuggestedRetailPrice,
        A_CIndicator,
        AgencyQualifierType
    }
    class SteinmartDataReader
    {
        string dir = @"C:\Users\rmiller\EDI_Data\steinmart\";

        // string dir = "I:/edi/steinMart/up/";

        StreamReader tr;
        // SalesOrder ord; try with out this
        ArrayList so_list;
        Hashtable partXref;
        Hashtable shipViaHash;
        string crTerms = "N30";
        string custId = "75070";
        public SteinmartDataReader()
        {
            initLookupStyle();
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            FileInfo[] fileListInfo = dirInfo.GetFiles();
            this.so_list = new ArrayList();
            foreach (FileSystemInfo fsi in fileListInfo)
            {
                string file = fsi.FullName;
                tr = new StreamReader(file);
                processFile();
            }
        }
        public ArrayList GetSOList()
        {
            return so_list;
        }
        void processFile()
        {
            string line = "";
            string lastStoreNo = "first";
            bool notFirstTime = false;
            SalesOrder so = new SalesOrder();
            while ((line = tr.ReadLine()) != null)
            {
                string[] split = line.Split(new Char[] { '\t' });
                string customerId = this.custId;
                string storeNoPre = split[(int)dicFmt.MarkForLocation];
                int storeInt = Convert.ToInt32(storeNoPre);
                string storeNo = storeInt.ToString();

                if (lastStoreNo != storeNo && notFirstTime)
                {
                    so_list.Add(so);
                    so = new SalesOrder();
                    notFirstTime = true;
                }
                so.CustomerID = customerId;
                so.ShipToNum = storeNo;
                string DCNumber = split[(int)dicFmt.ShipToStoreName];
                
                so.ediMarkingNotes = DCNumber + "--->  " + storeNo;
                so.RequestDateStr = split[(int)dicFmt.ShipDate];
                so.CancelDateStr = split[(int)dicFmt.CancelAfter];
                so.OrderDateStr = split[(int)dicFmt.PODate];
                so.RequestDate = smConvertStrToDate(so.RequestDateStr);
                so.NeedByDate = smConvertStrToDate(so.CancelDateStr);
                so.OrderDate = smConvertStrToDate(so.OrderDateStr);
                so.PoNo = split[(int)dicFmt.PONumber];
                so.ShipVia = this.getShipVia(storeNo);
                so.TermsCode = crTerms;

                bool processLine = true;
                string smPart = split[(int)dicFmt.SKUNumber];
                so.CustomerPart = smPart;

                try
                {
                    so.Upc = partXref[smPart].ToString();
                }
                catch
                {
                    // MessageBox.Show("This Steinmart UPC does not match "  + smPart);
                    processLine = false;
                }
                so.PartRevision = "0";
                so.OrderQty = Convert.ToDecimal(split[(int)dicFmt.Qty]);
                so.UnitPrice = Convert.ToDecimal(split[(int)dicFmt.UnitPrice]);

                if (processLine)
                {
                    so.postLine();
                }
                lastStoreNo = storeNo;
                notFirstTime = true;
            }

            // append order to collection            
            so_list.Add(so);
            so = new SalesOrder();
            notFirstTime = true;
        }
        public System.DateTime smConvertStrToDate(string dateStr)
        {
            string year = dateStr.Substring(6, 4);
            string month = dateStr.Substring(0, 2);
            string day = dateStr.Substring(3, 2);

            System.DateTime dateObj = new DateTime(Convert.ToInt32(year),
                Convert.ToInt32(month), Convert.ToInt32(day));
            return dateObj;
        }

        void initLookupStyle()
        {
            partXref = new Hashtable();
            partXref.Add("15908403", "757026153448");
            partXref.Add("16694127", "757026136601");
            partXref.Add("16694135", "757026131477");
            partXref.Add("16694101", "757026161313");
            partXref.Add("21230842", "757026176102");
            partXref.Add("21230859", "757026167049");
            partXref.Add("21230867", "757026177062");
            partXref.Add("21230875", "757026167209");
            partXref.Add("21230883", "757026176614");
            partXref.Add("21230891", "757026176546");
            partXref.Add("21230909", "757026182592");
            partXref.Add("21230917", "757026183742");
            partXref.Add("21230925", "757026183766");
            partXref.Add("16694119", "757026161320");
            partXref.Add("21327549", "757026192676");
            partXref.Add("21327614", "757026192683");
            partXref.Add("21144076", "757026191952");
            partXref.Add("21144084", "757026191969");
            partXref.Add("21144092", "757026191976");
            partXref.Add("21144100", "757026192003");
            partXref.Add("21144118", "757026191945");
            partXref.Add("21144126", "757026185395");
            partXref.Add("21144134", "757026185388");
            partXref.Add("20771663", "757026186590");
            partXref.Add("20865952", "757026189683");
            partXref.Add("20865960", "757026189546");
            partXref.Add("21472568", "757026182356");
            partXref.Add("21144191", "757026191938");
            partXref.Add("21144423", "757026191983");
            partXref.Add("21144431", "757026191990");
            partXref.Add("21144449", "757026192010");
            partXref.Add("22927701", "757026170544");
            partXref.Add("22927719", "757026192515");
            partXref.Add("22927727", "757026192881");
            partXref.Add("22927735", "757026191594");
            partXref.Add("22927743", "757026194410");
            partXref.Add("22927750", "757026192669");
            partXref.Add("22927768", "757026191587");
            partXref.Add("22927776", "757026194588");
            partXref.Add("22927628", "757026117228");
            partXref.Add("22927610", "757026134508");
            partXref.Add("22927602", "757026192799");
            partXref.Add("22927594", "757026166554");
            partXref.Add("23473713", "757026198654");
            partXref.Add("23473820", "757026198623");
            partXref.Add("23473945", "757026198562");
            partXref.Add("23473952", "757026198487");
            partXref.Add("23474174", "757026198494");
            partXref.Add("23474299", "757026198500");
            partXref.Add("23474307", "757026198630");
            partXref.Add("23457245", "757026198555");
            partXref.Add("23457252", "757026198586");
            partXref.Add("23457260", "757026198616");
            partXref.Add("23457278", "757026198579");
            partXref.Add("41828526", "757026199330");
            partXref.Add("41828666", "757026199132");
            partXref.Add("41828690", "757026199293");
            partXref.Add("41828773", "757026199286");
            partXref.Add("41829193", "757026199248");
            partXref.Add("41829235", "757026166059");
            partXref.Add("41829755", "757026199224");
            partXref.Add("41830399", "757026197923");
            partXref.Add("41830449", "757026197930");
            partXref.Add("42077719", "757026200715");
            partXref.Add("42078071", "757026201941");
            partXref.Add("42078329", "757026200739");
            partXref.Add("42079392", "757026201873");
            partXref.Add("42079673", "757026200708");
            partXref.Add("42079707", "757026200746");
            partXref.Add("42070367", "757026202009");
            partXref.Add("42671750", "757026204799");
            partXref.Add("42671792", "757026197862");
            partXref.Add("42671859", "757026202467");
            partXref.Add("42671909", "757026194700");
            partXref.Add("42671982", "757026182264");
            partXref.Add("42672030", "757026166325");
            partXref.Add("42672113", "757026203174");
            partXref.Add("42672295", "757026199361");
            partXref.Add("42672360", "757026191587");
            partXref.Add("42672527", "757026194564");
            partXref.Add("42672550", "757026189522");
            partXref.Add("43181320", "757026201866");
            partXref.Add("43181114", "757026202979");
            partXref.Add("43181387", "757026208292");
            partXref.Add("43180850", "757026196322");
            partXref.Add("43180884", "757026194496");
            partXref.Add("43180900", "757026204799");
            partXref.Add("43180926", "757026197862");
            partXref.Add("43181072", "757026204713");
            partXref.Add("43181098", "757026202467");
            partXref.Add("43181346", "757026208308");
            partXref.Add("43398619", "757026208483");
            partXref.Add("43398882", "757026208490");
            partXref.Add("43399302", "757026208506");
            partXref.Add("43399401", "757026208513");
            partXref.Add("43407709", "757026208544");
            partXref.Add("43407725", "757026208551");
            partXref.Add("43407741", "757026208568");
            partXref.Add("43407766", "757026208599");
            partXref.Add("43407782", "757026208605");
            partXref.Add("43407808", "757026208612");
            partXref.Add("43407824", "757026208629");
            partXref.Add("43407840", "757026208636");
            partXref.Add("43407865", "757026208643");
            partXref.Add("43407881", "757026208650");
            partXref.Add("43408046", "757026208667");
            partXref.Add("43408186", "757026208674");
            partXref.Add("43408202", "757026208681");
            partXref.Add("43408236", "757026208698");
            partXref.Add("43408277", "757026208704");
            partXref.Add("43408400", "757026208711");
            partXref.Add("43408558", "757026208728");
            partXref.Add("43408632", "757026208735");
            partXref.Add("43408889", "757026208742");
            partXref.Add("43408962", "757026208834");
            partXref.Add("43409135", "757026208841");
            partXref.Add("43409218", "757026208858");
            partXref.Add("43409283", "757026208865");
            partXref.Add("43409879", "757026208889");
            partXref.Add("43410034", "757026208896");
            partXref.Add("43410166", "757026208902");
            partXref.Add("43410620", "757026197398");
            partXref.Add("43410828", "757026197718");
            partXref.Add("43410927", "757026197725");
            partXref.Add("43410984", "757026197749");
            partXref.Add("43411255", "757026197756");
            partXref.Add("43411685", "757026197763");
            partXref.Add("43411891", "757026201552");
            partXref.Add("43412030", "757026201576");
            partXref.Add("43412139", "757026201590");
            partXref.Add("43412295", "757026201613");
            partXref.Add("43412519", "757026201507");
            partXref.Add("43412691", "757026201521");
            partXref.Add("43413384", "757026155633");
            partXref.Add("43414374", "757026155640");
            partXref.Add("43414465", "757026155657");
            partXref.Add("43414549", "757026155664");
            partXref.Add("43414606", "757026155602");
            partXref.Add("43414697", "757026155626");
            partXref.Add("43414762", "757026155725");
            partXref.Add("43414887", "757026155732");
            partXref.Add("43414978", "757026155749");
            partXref.Add("43415033", "757026155671");
            partXref.Add("43415090", "757026155688");
            partXref.Add("43415124", "757026155695");
            partXref.Add("43415173", "757026211681");
            partXref.Add("43415215", "757026133631");
            partXref.Add("43415249", "757026121409");
            partXref.Add("44007391", "757026211117");
            partXref.Add("44007508", "757026211100");
            partXref.Add("44006773", "757026208308");
            partXref.Add("44006971", "757026208292");
            partXref.Add("44007045", "757026191587");
            partXref.Add("44007565", "757026209442");
            partXref.Add("44007631", "757026209459");
            partXref.Add("44850667", "757026161177");
            partXref.Add("44850634", "757026161153");
            partXref.Add("44850550", "757026174467");
            partXref.Add("44850899", "757026211049");
            partXref.Add("44850949", "757026211971");
            partXref.Add("44850576", "757026220379");
            partXref.Add("44850600", "757026194670");
            partXref.Add("44850691", "757026211063");
            partXref.Add("44851020", "757026216198");
            partXref.Add("44850923", "757026209459");
            partXref.Add("44851111", "757026211117");
            partXref.Add("44851145", "757026220447");
            partXref.Add("44850493", "757026211001");

            partXref.Add("45922796", "757026221932");
            partXref.Add("45922846", "757026216785");
            partXref.Add("45922937", "757026201866");
            partXref.Add("45923158", "757026208308");
            partXref.Add("45923240", "757026220447");
            partXref.Add("45923299", "757026189386");
            partXref.Add("45923489", "757026198388");
            partXref.Add("45923737", "757026220430");
            partXref.Add("45923836", "757026221871");
            partXref.Add("45923869", "757026221925");

            partXref.Add("46668463", "757026221949");
            partXref.Add("46668505", "757026225275");
            partXref.Add("46668653", "757026221857");
            partXref.Add("46668679", "757026225688");
            partXref.Add("46668737", "757026198388");
            partXref.Add("46672630", "757026216884");
            partXref.Add("46672820", "757026217133");
            partXref.Add("46672846", "757026216938");
            partXref.Add("46672903", "757026217188");
            partXref.Add("46672937", "757026216952");
            partXref.Add("46672994", "757026217201");
            partXref.Add("46673026", "757026217102");
            partXref.Add("46673083", "757026217355");
            partXref.Add("46673166", "757026216990");
            partXref.Add("46673182", "757026217249");
            partXref.Add("46673240", "757026217096");
            partXref.Add("46673265", "757026217348");
            partXref.Add("46673281", "757026217041");
            partXref.Add("46673463", "757026217294");
            partXref.Add("46673505", "757026217058");
            partXref.Add("46673596", "757026217300");
            partXref.Add("46673703", "757026217072");
            partXref.Add("46673869", "757026217324");
            partXref.Add("48035976", "757026220430");
            partXref.Add("48036115", "757026203013");
            partXref.Add("48036149", "757026232211");
            partXref.Add("48036164", "757026232068");
            partXref.Add("48036180", "757026232297");
            partXref.Add("48036206", "757026232235");
            partXref.Add("48036222", "757026232105");
            partXref.Add("48036248", "757026232242");
            partXref.Add("48036263", "757026232112");
            partXref.Add("48048896", "757026198388");

            partXref.Add("49732944", "757026233683");
            partXref.Add("49733017", "757026239005");
            partXref.Add("49735525", "757026238978");
            partXref.Add("49735640", "757026239098");
            partXref.Add("49735822", "757026237247");
            partXref.Add("49735897", "757026237216");
            partXref.Add("49736176", "757026237186");
            partXref.Add("49736283", "757026239036");
            partXref.Add("49736937", "757026238961");
            partXref.Add("49736994", "757026237209");
            partXref.Add("49737117", "757026237223");
            partXref.Add("50277029", "757026232181");
            partXref.Add("50277052", "757026239173");
            partXref.Add("50277086", "757026239951");
            partXref.Add("52498987", "757026273894");
            partXref.Add("52499035", "757026266704");
            partXref.Add("52499621", "757026265578");
            partXref.Add("52499712", "757026263512");
            partXref.Add("52499837", "757026273948");
            partXref.Add("52499852", "757026273825");
            // added for new order 12 feb 14  not tested

            partXref.Add("52707528", "757026262034");
            partXref.Add("52709177", "757026266681");
            partXref.Add("52709334", "757026273962");
            partXref.Add("52709441", "757026266292");
            partXref.Add("52709565", "757026262041");
            partXref.Add("52709714", "757026266698");
            partXref.Add("52707544", "757026274259");
            partXref.Add("52707585", "757026261969");
            partXref.Add("52828910", "757026273832");
            partXref.Add("52720638", "757026263482");
        }
        string getShipVia(string store)
        {
            string shipVia = "CPKP";
            return shipVia;
        }
        string getCaUpc(string smPart)
        {
            string upc = partXref[smPart].ToString();
            return upc;
        }
    }
}



