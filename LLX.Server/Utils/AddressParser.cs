using System.Text.RegularExpressions;
using LLX.Server.Models.DTOs;

namespace LLX.Server.Utils;

/// <summary>
/// 地址智能识别工具
/// </summary>
public static class AddressParser
{
    // 省份列表
    private static readonly string[] Provinces = new[]
    {
        "北京市", "天津市", "上海市", "重庆市",
        "河北省", "山西省", "辽宁省", "吉林省", "黑龙江省",
        "江苏省", "浙江省", "安徽省", "福建省", "江西省", "山东省",
        "河南省", "湖北省", "湖南省", "广东省", "海南省",
        "四川省", "贵州省", "云南省", "陕西省", "甘肃省", "青海省",
        "内蒙古自治区", "广西壮族自治区", "西藏自治区", "宁夏回族自治区", "新疆维吾尔自治区",
        "香港特别行政区", "澳门特别行政区", "台湾省"
    };

    /// <summary>
    /// 解析地址文本
    /// </summary>
    /// <param name="text">地址文本</param>
    /// <returns>解析结果</returns>
    public static ParsedAddressDto Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("地址文本不能为空");

        var result = new ParsedAddressDto();
        
        // 清理文本：去除多余空格、换行等
        var cleanText = Regex.Replace(text, @"\s+", " ").Trim();
        
        // 1. 提取姓名（2-4个汉字，在开头或"收货人"之后）
        var nameMatch = Regex.Match(cleanText, @"(?:收货人[:：\s]*)?(?<name>[\u4e00-\u9fa5]{2,4})(?:先生|女士)?");
        if (nameMatch.Success)
        {
            result.Name = nameMatch.Groups["name"].Value;
            cleanText = cleanText.Replace(nameMatch.Value, "");
        }

        // 2. 提取电话（11位手机号或带区号的固定电话）
        var phoneMatch = Regex.Match(cleanText, @"(?:电话|手机|联系方式)?[:：\s]*(?<phone>1[3-9]\d{9}|\d{3,4}-?\d{7,8})");
        if (phoneMatch.Success)
        {
            result.Phone = phoneMatch.Groups["phone"].Value.Replace("-", "");
            cleanText = cleanText.Replace(phoneMatch.Value, "");
        }

        // 3. 提取省份
        foreach (var province in Provinces)
        {
            if (cleanText.Contains(province))
            {
                result.Province = province;
                cleanText = cleanText.Replace(province, "");
                break;
            }
            // 尝试去掉"省"、"市"匹配
            var shortProvince = province.Replace("省", "").Replace("市", "").Replace("自治区", "").Replace("特别行政区", "");
            if (cleanText.Contains(shortProvince))
            {
                result.Province = province;
                cleanText = cleanText.Replace(shortProvince, "");
                break;
            }
        }

        // 4. 提取城市
        var cityMatch = Regex.Match(cleanText, @"(?<city>[\u4e00-\u9fa5]{2,10}?(?:市|地区|州|盟))");
        if (cityMatch.Success)
        {
            result.City = cityMatch.Value;
            cleanText = cleanText.Replace(cityMatch.Value, "");
        }

        // 5. 提取区县
        var districtMatch = Regex.Match(cleanText, @"(?<district>[\u4e00-\u9fa5]{2,10}?(?:区|县|市|旗))");
        if (districtMatch.Success)
        {
            result.District = districtMatch.Value;
            cleanText = cleanText.Replace(districtMatch.Value, "");
        }

        // 6. 剩余部分作为详细地址
        result.Detail = cleanText.Trim()
            .Replace("地址", "")
            .Replace(":", "")
            .Replace("：", "")
            .Trim();

        return result;
    }
}
