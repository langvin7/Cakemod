using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;



public class CakeKeywords
{
    // 自定义枚举的名字。最终会变成{前缀}-{枚举值大写}的形式，例如TEST-UNIQUE
    [CustomEnum("PLAYFROMDRAWPILE")]
    // 放在原版卡牌描述的位置，这里是卡牌描述的前面
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword PlayFromDrawPile;
}