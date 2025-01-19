using UnityEngine;

namespace Utilities.Collections
{
    public class CollectionsTest : MonoBehaviour
    {
        void Start()
        {
            var biDic = new BiDictionary<string, int>();

            biDic.Add("1", 1);
            biDic.Add("2", 2);
            biDic.Add("3", 3);
            biDic.Add("4", 4);

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
