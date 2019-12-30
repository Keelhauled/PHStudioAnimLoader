using BepInEx.Logging;
using Studio;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StudioAnimLoader
{
    public class LoaderComponent : MonoBehaviour
    {
        private const string fileSuffixGrpF = "FemaleAnimeGroup_";
        private const string fileSuffixCatF = "FemaleAnimeCategory_";
        private const string fileSuffixAnimF = "FemaleAnime_";
        private const string fileSuffixHAnimF = "FemaleHAnime_";
        private const string fileSuffixGrpM = "MaleAnimeGroup_";
        private const string fileSuffixCatM = "MaleAnimeCategory_";
        private const string fileSuffixAnimM = "MaleAnime_";
        private const string fileSuffixHAnimM = "MaleHAnime_";

        //voice
        private const string fileSuffixVoiceGroup = "VoiceGroup_";
        private const string fileSuffixVoiceCategory = "VoiceCategory_";
        private const string fileSuffixVoice = "Voice_";

        private Dictionary<string, Dictionary<string, List<string[]>>> dicAllFileArgs = new Dictionary<string, Dictionary<string, List<string[]>>>();

        private Info info;
        private int groupOffset;
        private bool force;
        private string dir;
        private string extDir = "";
        private string groupSuffix = "";

        private void Start()
        {
            var assDir = Path.GetDirectoryName(GetType().Assembly.Location);
            dir = Path.GetFullPath(Path.Combine(assDir, StudioAnimLoader.InfoDir.Value));
            groupOffset = StudioAnimLoader.GroupOffset.Value;
            groupSuffix = StudioAnimLoader.GroupSuffix.Value;
            force = StudioAnimLoader.Overwrite.Value;
            info = Singleton<Info>.Instance;

            if(!Directory.Exists(dir) || !Directory.Exists(StudioAnimLoader.OtherGameDir.Value))
            {
                StudioAnimLoader.Logger.Log(LogLevel.Message | LogLevel.Warning, "Aborting. Problem with InfoDir or OtherGameDir settings.");
                return;
            }

            Uri phPath = new Uri(Application.dataPath + @"\abdata\");
            Uri extPath = new Uri(StudioAnimLoader.OtherGameDir.Value + @"\abdata\");
            try
            {
                extDir = phPath.MakeRelativeUri(extPath).ToString();
            }
            catch(Exception e)
            {
                StudioAnimLoader.Logger.LogError($"*** StudioAnimLoader InfoDir or OtherGameDir Problem? ***\n{e}\n**********");
                StudioAnimLoader.Logger.Log(LogLevel.Message | LogLevel.Warning, "Aborting. Problem with InfoDir or OtherGameDir settings.");
                return;
            }

            LoadAll();
        }

        private void LoadAll()
        {
            LoadFiles();

            //Anim
            LoadGroup(AnimeGroupList.SEX.Female, true);
            LoadGroup(AnimeGroupList.SEX.Male, true);

            LoadCategory(AnimeGroupList.SEX.Female, true);
            LoadCategory(AnimeGroupList.SEX.Male, true);

            LoadAnim(AnimeGroupList.SEX.Female, false);
            LoadAnim(AnimeGroupList.SEX.Male, false);

            LoadAnim(AnimeGroupList.SEX.Female, true);
            LoadAnim(AnimeGroupList.SEX.Male, true);

            //Voice
            LoadGroup(AnimeGroupList.SEX.Female, false);
            LoadCategory(AnimeGroupList.SEX.Female, false);
            LoadVoice();

            dicAllFileArgs = null;
        }

        //Read All Files

        private void LoadFiles()
        {
            string[] suffixes = new string[] { fileSuffixGrpF, fileSuffixGrpM, fileSuffixCatF, fileSuffixCatM, fileSuffixAnimF, fileSuffixAnimM, fileSuffixHAnimF, fileSuffixHAnimM,
            fileSuffixVoiceGroup, fileSuffixVoiceCategory, fileSuffixVoice};

            for(int i = 0; i < suffixes.Length; i++)
            {
                dicAllFileArgs.Add(suffixes[i], _LoadFiles(dir, suffixes[i] + "*.MonoBehaviour"));
            }
        }

        private string[] ParseSB3UList(string text)
        {
            try
            {
                return text.Replace(">", "").Remove(0, 1).Split(new string[] { "<" }, 0);
            }
            catch
            {
                return null;
            }
        }

        private Dictionary<string, List<string[]>> _LoadFiles(string dir, string pattern)
        {
            Dictionary<string, List<string[]>> dicFileArgs = new Dictionary<string, List<string[]>>();
            string[] files = Directory.GetFiles(dir, pattern);
            if(files == null) return null;

            List<string[]> argsList;

            foreach(string file in files)
            {
                if(!File.Exists(file)) continue;
                argsList = new List<string[]>();

                using(StreamReader streamReader = File.OpenText(file))
                {
                    string line = "";
                    string[] args;
                    while((line = streamReader.ReadLine()) != null)
                    {
                        try
                        {
                            args = ParseSB3UList(line);

                            if(args != null && int.TryParse(args[0], out int n) && args.Length > 1)
                            {
                                argsList.Add(args);
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                dicFileArgs.Add(Path.GetFileNameWithoutExtension(file), argsList);
            }

            return dicFileArgs;
        }

        //Import to Studio Dictionary

        private void LoadGroup(AnimeGroupList.SEX sex, bool isAnim)
        {
            Dictionary<int, Info.GroupInfo> dicInfo;
            Dictionary<string, List<string[]>> dicArgs;
            int index;
            string pattern;

            switch(sex)
            {
                case AnimeGroupList.SEX.Female:
                    dicInfo = info.dicFAGroupCategory;
                    pattern = fileSuffixGrpF;
                    break;
                case AnimeGroupList.SEX.Male:
                    dicInfo = info.dicMAGroupCategory;
                    pattern = fileSuffixGrpM;
                    break;
                default:
                    return;
            }

            if(!isAnim)
            {
                dicInfo = info.dicVoiceGroupCategory;
                pattern = fileSuffixVoiceGroup;
            }

            dicArgs = dicAllFileArgs[pattern];

            if(dicArgs == null) return;

            foreach(string fileName in dicArgs.Keys)
            {
                foreach(string[] args in dicArgs[fileName])
                {
                    if(int.TryParse(args[0], out int baseIndex))
                    {
                        index = baseIndex + groupOffset;
                        if(dicInfo.ContainsKey(index))
                        {
                            if(force)
                            {
                                dicInfo[index].name = groupSuffix + args[1];
                            }
                        }
                        else
                        {
                            dicInfo.Add(index, new Info.GroupInfo()
                            {
                                name = groupSuffix + args[1],
                                dicCategory = new Dictionary<int, string>()
                            });
                        }
                    }
                }

            }
        }

        private void LoadCategory(AnimeGroupList.SEX sex, bool isAnim)
        {
            Dictionary<int, Info.GroupInfo> dicInfo;
            Dictionary<string, List<string[]>> dicArgs;
            int index;
            string pattern;

            switch(sex)
            {
                case AnimeGroupList.SEX.Female:
                    dicInfo = info.dicFAGroupCategory;
                    pattern = fileSuffixCatF;
                    break;
                case AnimeGroupList.SEX.Male:
                    dicInfo = info.dicMAGroupCategory;
                    pattern = fileSuffixCatM;
                    break;
                default:
                    return;
            }

            if(!isAnim)
            {
                dicInfo = info.dicVoiceGroupCategory;
                pattern = fileSuffixVoiceCategory;
            }

            dicArgs = dicAllFileArgs[pattern];
            if(dicArgs == null) return;

            foreach(string fileName in dicArgs.Keys)
            {
                // 00_00 のはず。
                string[] ss = fileName.Replace(pattern, "").Split(new string[] { "_" }, 0);

                if(int.TryParse(ss[0], out int baseIndex))
                {
                    index = baseIndex + groupOffset;

                    foreach(string[] args in dicArgs[fileName])
                    {

                        if(int.TryParse(args[0], out int catIndex))
                        {
                            if(dicInfo.ContainsKey(index))
                            {
                                if(dicInfo[index].dicCategory.ContainsKey(catIndex))
                                {
                                    if(force)
                                    {
                                        dicInfo[index].dicCategory[catIndex] = args[1];
                                    }
                                }
                                else
                                {
                                    dicInfo[index].dicCategory.Add(catIndex, args[1]);
                                }
                            }
                            else
                            {
                                //グループが先に用意されてない場合　ひとまず無視
                            }
                        }

                    }

                }

            }
        }

        private void LoadAnim(AnimeGroupList.SEX sex, bool isH)
        {
            Dictionary<int, Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>> dicInfo;
            Dictionary<string, List<string[]>> dicArgs;
            int index;
            string pattern;

            switch(sex)
            {
                case AnimeGroupList.SEX.Female:
                    dicInfo = info.dicFemaleAnimeLoadInfo;
                    if(isH)
                    {
                        pattern = fileSuffixHAnimF;
                    }
                    else
                    {
                        pattern = fileSuffixAnimF;
                    }
                    break;
                case AnimeGroupList.SEX.Male:
                    dicInfo = info.dicMaleAnimeLoadInfo;
                    if(isH)
                    {
                        pattern = fileSuffixHAnimM;
                    }
                    else
                    {
                        pattern = fileSuffixAnimM;
                    }
                    break;
                default:
                    return;
            }

            dicArgs = dicAllFileArgs[pattern];
            if(dicArgs == null) return;

            foreach(string fileName in dicArgs.Keys)
            {
                // 00_00_00 のはず。
                //または、各行からの方が良いか？ <- でも無駄が多くなる？
                string[] ss = fileName.Replace(pattern, "").Split(new string[] { "_" }, 0);

                if(int.TryParse(ss[0], out int baseIndex) && int.TryParse(ss[1], out int catIndex))
                {
                    index = baseIndex + groupOffset;

                    foreach(string[] args in dicArgs[fileName])
                    {
                        //アニメの場合、グループもカテゴリーもインデックスがなければ追加
                        if(int.TryParse(args[0], out int clipIndex))
                        {
                            if(!dicInfo.ContainsKey(index)) dicInfo.Add(index, new Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>());
                            if(!dicInfo[index].ContainsKey(catIndex)) dicInfo[index].Add(catIndex, new Dictionary<int, Info.AnimeLoadInfo>());

                            //アニメ情報入れる
                            //<0管理番号><1大きい項目><2中間項目><3表示名><4バンドルパス><5ファイル名><6クリップ名><7BreastLayerの有り無し><8揺れ制御左><9揺れ制御右><10><11アイテム有り無し><管理番号><バンドルパス><ファイル名><付ける場所><管理番号><バンドルパス><ファイル名><付ける場所>
                            Info.AnimeLoadInfo animInfo = null;
                            try
                            {
                                animInfo = new Info.AnimeLoadInfo()
                                {
                                    name = args[3],
                                    bundlePath = extDir + args[4],
                                    fileName = args[5],
                                    clip = args[6],
                                    isBreastLayer = args[7] == "True",
                                    isMotion = true,
                                    isHAnime = isH,
                                    isScale = false
                                };
                            }
                            catch
                            {
                                animInfo = null;
                            }

                            if(dicInfo[index][catIndex].ContainsKey(clipIndex))
                            {
                                if(force) dicInfo[index][catIndex][clipIndex] = animInfo;
                            }
                            else
                            {
                                dicInfo[index][catIndex].Add(clipIndex, animInfo);
                            }
                        }
                    }

                }

            }
        }

        //Voice

        private void LoadVoice()
        {
            Dictionary<int, Dictionary<int, Dictionary<int, Info.LoadCommonInfo>>> dicInfo;
            Dictionary<string, List<string[]>> dicArgs;
            int index;
            string pattern;

            dicInfo = info.dicVoiceLoadInfo;
            pattern = fileSuffixVoice;

            dicArgs = dicAllFileArgs[pattern];
            if(dicArgs == null) return;

            foreach(string fileName in dicArgs.Keys)
            {
                // 00_00_00 のはず。
                //または、各行からの方が良いか？ <- でも無駄が多くなる？
                string[] ss = fileName.Replace(pattern, "").Split(new string[] { "_" }, 0);

                if(int.TryParse(ss[0], out int baseIndex) && int.TryParse(ss[1], out int catIndex))
                {
                    index = baseIndex + groupOffset;

                    foreach(string[] args in dicArgs[fileName])
                    {
                        //ボイスの場合、グループもカテゴリーもインデックスがなければ追加
                        if(int.TryParse(args[0], out int clipIndex))
                        {
                            if(!dicInfo.ContainsKey(index)) dicInfo.Add(index, new Dictionary<int, Dictionary<int, Info.LoadCommonInfo>>());
                            if(!dicInfo[index].ContainsKey(catIndex)) dicInfo[index].Add(catIndex, new Dictionary<int, Info.LoadCommonInfo>());

                            //ボイス情報入れる
                            //<0管理番号><1大きい項目><2中間項目><3表示名><4バンドルパス><5ファイル名>

                            Info.LoadCommonInfo voiceInfo = null;

                            try
                            {
                                voiceInfo = new Info.LoadCommonInfo()
                                {
                                    name = args[3],
                                    bundlePath = extDir + args[4],
                                    fileName = args[5],
                                };
                            }
                            catch
                            {
                                voiceInfo = null;
                            }

                            if(dicInfo[index][catIndex].ContainsKey(clipIndex))
                            {
                                if(force) dicInfo[index][catIndex][clipIndex] = voiceInfo;
                            }
                            else
                            {
                                dicInfo[index][catIndex].Add(clipIndex, voiceInfo);
                            }
                        }
                    }

                }

            }
        }
    }
}
