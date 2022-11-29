//审核状态
function shenhe_state(xulie, zhuangtai) {
    //alert(zhuangtai);
    switch (xulie) {
        case -1: return "<Image src='/Scripts/easyui/themes/icons/question.png' Title='未提交' />"; 
        case 0: return "<Image src='/Scripts/easyui/themes/icons/tijiao.png' Title='已提交，一审中' />";
        case 1: return "<Image src='/Scripts/easyui/themes/icons/zoom.png' Title='二审中' />";
        case 2:
            if (zhuangtai == "通过") {
                return "<Image src='/Scripts/easyui/themes/icons/ok.png' Title='通过' />";
            } else {
                return "<Image src='/Scripts/easyui/themes/icons/no.png' Title='未通过' />"; 
            }
        case 3: return "<Image src='/Scripts/easyui/themes/icons/no.png' Title='未通过' />";
        //default: return "<Image src='/Scripts/easyui/themes/icons/no.png' Title='未通过' />";
    }
}

//function liucheng_rizhi(value, row, index) {
//    var shenhe = row.shenhezhuangtai;
//    var goumaizhuangtai = row.goumaizhuangtai;
//    var dingdanshouhuo = row.dingdanshouhuo;
//    switch (shenhe) {
//        case "通过":
//            if (dingdanshouhuo == "已收货") {
//                return "<Image src='/Scripts/easyui/themes/icons/basket.png' Title='已收货' onclick='rizhi(" + index + ")'/>";
//            } else {
//                if (goumaizhuangtai == "已购买") {
//                    return "<Image src='/Scripts/easyui/themes/icons/cart_put.png' Title='已购买' onclick='rizhi(" + index + ")'/>";
//                } else {
//                    return "<Image src='/Scripts/easyui/themes/icons/ok.png' Title='通过' onclick='rizhi(" + index + ")'/>";
//                }
//            }
//            break;
//        case "未通过": return "<Image src='/Scripts/easyui/themes/icons/no.png' Title='未通过' onclick='rizhi(" + index + ")'/>";
//            break;
//        case "审核中": return "<Image src='/Scripts/easyui/themes/icons/zoom.png' Title='审核中' onclick='rizhi(" + index + ")'/>";
//            break;
//        case "已提交": return "<Image src='/Scripts/easyui/themes/icons/tijiao.png' Title='已提交' onclick='rizhi(" + index + ")'/>";
//            break;
//        case "未提交": return "<Image src='/Scripts/easyui/themes/icons/question.png' Title='未提交' onclick='rizhi(" + index + ")'/>";
//            break;
//    }
//};