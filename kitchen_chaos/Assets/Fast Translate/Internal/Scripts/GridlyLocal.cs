using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FastTranslate.Internal;
using TMPro;
namespace FastTranslate
{
    public enum Languages
    {
        enUS,
        arSA,
        frFR,
        zhCN,
        zhTW,
        deDE,
        itIT,
        jaJP,
        koKR,
        plPL,
        ptBR,
        ruRU,
        esES,
        esMX,
        caES,
        bnBD,
        bgBG,
        zhHK,
        afZA,
        arAE,
        arBH,
        arDZ,
        arEG,
        arIQ,
        arJO,
        arKW,
        arLB,
        arLY,
        arMA,
        arOM,
        arQA,
        arSY,
        arTN,
        arYE,
        azAZ,
        beBY,
        bsBA,
        csCZ,
        cyGB,
        daDK,
        deAT,
        deCH,
        deLI,
        deLU,
        dvMV,
        elGR,
        enAU,
        enBZ,
        enCA,
        enGB,
        enIE,
        enJM,
        enNZ,
        enPH,
        enTT,
        enZA,
        enZW,
        esAR,
        esBO,
        esCL,
        esCO,
        esCR,
        esDO,
        esEC,
        esGT,
        esHN,
        esNI,
        esPA,
        esPE,
        esPR,
        esPY,
        esSV,
        esUY,
        esVE,
        etEE,
        euES,
        faIR,
        fiFI,
        foFO,
        frBE,
        frCA,
        frCH,
        frLU,
        frMC,
        glES,
        guIN,
        heIL,
        hiIN,
        hrBA,
        hrHR,
        huHU,
        hyAM,
        idID,
        isIS,
        itCH,
        kaGE,
        kkKZ,
        knIN,
        kyKG,
        ltLT,
        lvLV,
        miNZ,
        mkMK,
        mnMN,
        mrIN,
        msBN,
        msMY,
        mtMT,
        nbNO,
        nlBE,
        nlNL,
        nnNO,
        nsZA,
        paIN,
        psAR,
        ptPT,
        quBO,
        quEC,
        quPE,
        roRO,
        saIN,
        seFI,
        seNO,
        seSE,
        skSK,
        siSI,
        sqAL,
        srBA,
        svFI,
        svSE,
        swKE,
        taIN,
        teIN,
        thTH,
        tlPH,
        tnZA,
        trTR,
        ttRU,
        ukUA,
        urPK,
        uzUZ,
        viVN,
        xhZA,
        zhMO,
        zhSG,
        zuZA,
    }

    public static class FTranslate
    {

        static LangSupport targetLanguage => Project.singleton.targetLanguage;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="recordID"></param>
        /// <returns>translated text with the current language</returns>
        public static string GetStringData(string table, string recordID)
        {
            try
            {
                Record record = Project.singleton
                .grids.Find(x => x.nameGrid == table)
                .records.Find(x => x.recordID == recordID);

                

                foreach (var column in record.columns)
                {
                    try
                    {
                        //Debug.Log(Project.singleton.targetLanguage.languagesSuport.ToString());
                        if (column.columnID == targetLanguage.languagesSuport.ToString())
                        {
                            return column.text;
                        }
                    }
                    catch // try to return other language if cant found target language
                    {
                        Debug.Log("cant found: " + recordID + " | code:" + Project.singleton.targetLanguage.languagesSuport.ToString());
                        foreach(var i in Project.singleton.langSupports)
                        {
                            if (column.columnID == i.languagesSuport.ToString())
                            {
                                return column.text;
                            }
                        }
                    }
                }

            }
            catch
            {
                //Debug.Log("Path does not exist. Please make sure you entered the correct path format, and added data");
            }

            return "";
        }

        public static void SetTMPro(TextMeshProUGUI tmPro,string fontName,string table, string record){
            tmPro.text = GetStringData(table, record);
            tmPro.font = Project.singleton.GetFontAsset(fontName).tmFont;
        }

        public static string GetStringData(string table, int index){
            try
            {
                Record record = Project.singleton
                .grids.Find(x => x.nameGrid == table)
                .records[index];

                foreach (var column in record.columns)
                {
                    try
                    {
                        //Debug.Log(Project.singleton.targetLanguage.languagesSuport.ToString());
                        if (column.columnID == Project.singleton.targetLanguage.languagesSuport.ToString())
                        {
                            return column.text;
                        }
                    }
                    catch // try to return other language if cant found target language
                    {
                        Debug.Log("cant found: " + index + " | code:" + Project.singleton.targetLanguage.languagesSuport.ToString());
                        foreach (var i in Project.singleton.langSupports)
                        {
                            if (column.columnID == i.languagesSuport.ToString())
                            {
                                return column.text;
                            }
                        }
                    }
                }

            }
            catch
            {
                //Debug.Log("Path does not exist. Please make sure you entered the correct path format, and added data");
            }

            return "";
        }

        public static T LoadObject<T>(string table, string recordID) where T : UnityEngine.Object
        {
            var strData = GetStringData(table, recordID); 
            return Resources.Load<T>(strData);
        }

        public static TMP_FontAsset GetTMPFont(string fontName){
            var font = Project.singleton.GetFontAsset(fontName);
            return font.tmFont;
        }  

        public static Font GetFont(string fontName){
            var font = Project.singleton.GetFontAsset(fontName);
            return font.font;
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <returns>return all supported language</returns>
        public static List<LangSupport> GetAllSupportedLanguage(){
            return Project.singleton.langSupports;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lang"></param>
        /// <returns>set the current language</returns>
        public static void SetLanguage(string lang){
            Project.singleton.SetChosenLanguageCode(lang);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="language"></param>
        /// <returns>set the current language</returns>
        public static void SetLanguage(Languages language)
        {
            Project.singleton.SetChosenLanguageCode(language);
        }

        public static string AddParam(string input,string param, string result)
        {
            return input.Replace("{"+param+"}",result);
        }

        public static string AddParams(string input, List<string> param, List<string> result)
        {
            if(param.Count != result.Count)
            {
                Debug.LogError("param and result count not match");
                return input;
            }
            for(int i = 0; i < param.Count; i++)
            {
                input = input.Replace("{" + param[i] + "}", result[i]);
            }
            return input;
        }

        public static List<string> GetAllKey(string table){
            List<string> keys = new List<string>();
            try
            {
                foreach (var i in Project.singleton.grids.Find(x => x.nameGrid == table).records)
                {
                    keys.Add(i.recordID);
                }
            }
            catch
            {
                Debug.LogError("cant found table: " + table);
            }
            return keys;
        }
        
    }



}
