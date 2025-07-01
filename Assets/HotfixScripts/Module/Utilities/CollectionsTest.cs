using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Collections
{
    public class CollectionsTest : MonoBehaviour
    {
        void Start()
        {
            var biDic = new BiDictionary<string, int>
            {
                { "1", 1 },
                { "2", 2 },
                { "3", 3 },
                { "4", 4 }
            };

            print($"{biDic["1"]} : {biDic[1]}");
            print($"{biDic["2"]} : {biDic[2]}");
            print($"{biDic["3"]} : {biDic[3]}");

            biDic["1"] = 2;
            print($"{biDic["1"]} : {biDic[2]}");
            print($"{biDic["2"]} : {biDic[2]}");

            print("==========");

            foreach (var item in biDic)
            {
                print($"{item.Key} : {item.Value}");
            }
        }
    }
}
