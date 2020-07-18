/*
compile by protobuf, please don't edit it manually. 
Any problem please contact tongxuehu@gmail.com, thx.
*/

using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Lib.Net;

namespace Assets.Scripts.Define
{
    // ������ö��
    public enum AttributeName
    {
        kFrontSpeedMax = 10001,   // ��ǰ������ٶ�
        kBackSpeedMax = 10002,   // ���������ٶ�
        kRightSpeedMax = 10003,   // ����������ٶ�
        kLeftSpeedMax = 10004,   // ����������ٶ�
        kUpSpeedMax = 10005,   // ����������ٶ�
        kDownSpeedMax = 10006,   // ����������ٶ�
        kFrontSpeedAccelerate = 10007,   // ��ǰ�߼��ٶ�
        kBackSpeedAccelerate = 10008,   // ����߼��ٶ�
        kRightSpeedAccelerate = 10009,   // �����߼��ٶ�
        kLeftSpeedAccelerate = 10010,   // �����߼��ٶ�
        kUpSpeedAccelerate = 10011,   // �����߼��ٶ�
        kDownSpeedAccelerate = 10012,   // �����߼��ٶ�
        kFrontBackSpeedDecelerate = 10013,   // ǰ���������ٶ�
        kLeftRightSpeedDecelerate = 10014,   // �����������ٶ�
        kUpDownSpeedDecelerate = 10015,   // �����������ٶ�
        kXAngleSpeedMax = 10016,   // X�������ٶ�
        kYAngleSpeedMax = 10017,   // Y�������ٶ�
        kZAngleSpeedMax = 10018,   // Z�������ٶ�
        kXAngleSpeedAccelerate = 10019,   // X��Ǽ��ٶ�
        kYAngleSpeedAccelerate = 10020,   // Y��Ǽ��ٶ�
        kZAngleSpeedAccelerate = 10021,   // Z��Ǽ��ٶ�
        kXAngleSpeedDecelerate = 10022,   // X�������Ǽ��ٶ�
        kYAngleSpeedDecelerate = 10023,   // Y�������Ǽ��ٶ�
        kZAngleSpeedDecelerate = 10024,   // Z�������Ǽ��ٶ�
        kJumpSpeedBegine = 10025,   // ԾǨ��ʼ�ٶ�
        kJumpSpeedAccelerate = 10026,   // ԾǨ���ٶ�
        kJumpSpeedDecelerate = 10027,   // ԾǨ���ٶ�
        kXMaxAngle = 10028,   // X��������
        kYMaxAngle = 10029,   // Y��������
        kZMaxAngle = 10030,   // Z��������
        kXMimicryMaxAngleAccelerate = 10031,   // ��̬X�����Ǽ��ٶ�
        kYMimicryMaxAngleAccelerate = 10032,   // ��̬Y�����Ǽ��ٶ�
        kZMimicryMaxAngleAccelerate = 10033,   // ��̬Z�����Ǽ��ٶ�
        kXMimicryMaxAngle = 10034,   // ��̬X�����Ƕ�
        kYMimicryMaxAngle = 10035,   // ��̬Y�����Ƕ�
        kZMimicryMaxAngle = 10036,   // ��̬Z�����Ƕ�
        kFightFrontSpeedMax = 10101,   // ս����ǰ������ٶ�
        kFightBackSpeedMax = 10102,   // ս�����������ٶ�
        kFightRightSpeedMax = 10103,   // ս������������ٶ�
        kFightLeftSpeedMax = 10104,   // ս������������ٶ�
        kFightUpSpeedMax = 10105,   // ս������������ٶ�
        kFightDownSpeedMax = 10106,   // ս������������ٶ�
        kFightFrontSpeedAccelerate = 10107,   // ս����ǰ�߼��ٶ�
        kFightBackSpeedAccelerate = 10108,   // ս������߼��ٶ�
        kFightRightSpeedAccelerate = 10109,   // ս�������߼��ٶ�
        kFightLeftSpeedAccelerate = 10110,   // ս�������߼��ٶ�
        kFightUpSpeedAccelerate = 10111,   // ս�������߼��ٶ�
        kFightDownSpeedAccelerate = 10112,   // ս�������߼��ٶ�
        kFightFrontBackSpeedDecelerate = 10113,   // ս��ǰ���������ٶ�
        kFightLeftRightSpeedDecelerate = 10114,   // ս�������������ٶ�
        kFightUpDownSpeedDecelerate = 10115,   // ս�������������ٶ�
        kFightXMaxAngleSpeed = 10116,   // ս��X�������ٶ�
        kFightYMaxAngleSpeed = 10117,   // ս��Y�������ٶ�
        kFightZMaxAngleSpeed = 10118,   // ս��Z�������ٶ�
        kFightXAngleSpeedAccelerate = 10119,   // ս��X��Ǽ��ٶ�
        kFightYAngleSpeedAccelerate = 10120,   // ս��Y��Ǽ��ٶ�
        kFightZAngleSpeedAccelerate = 10121,   // ս��Z��Ǽ��ٶ�
        kFightXAngleSpeedDecelerate = 10122,   // ս��X�������Ǽ��ٶ�
        kFightYAngleSpeedDecelerate = 10123,   // ս��Y�������Ǽ��ٶ�
        kFightZAngleSpeedDecelerate = 10124,   // ս��Z�������Ǽ��ٶ�
        kFightXMaxAngle = 10125,   // ս��X��������
        kFightYMaxAngle = 10126,   // ս��Y��������
        kFightZMaxAngle = 10127,   // ս��Z��������
        kXFightMimicryAngleSpeedAccelerate = 10128,   // ս����̬X�����Ǽ��ٶ�
        kYFightMimicryAngleSpeedAccelerate = 10129,   // ս����̬Y�����Ǽ��ٶ�
        kZFightMimicryAngleSpeedAccelerate = 10130,   // ս����̬Z�����Ǽ��ٶ�
        kXFightMimicryAngle = 10131,   // ս����̬X�����Ƕ�
        kYFightMimicryAngle = 10132,   // ս����̬Y�����Ƕ�
        kZFightMimicryAngle = 10133,   // ս����̬Z�����Ƕ�
        kOverloadFrontSpeedMax = 10201,   // ������ǰ������ٶ�
        kOverloadBackSpeedMax = 10202,   // �������������ٶ�
        kOverloadRightSpeedMax = 10203,   // ��������������ٶ�
        kOverloadLeftSpeedMax = 10204,   // ��������������ٶ�
        kOverloadUpSpeedMax = 10205,   // ��������������ٶ�
        kOverloadDownSpeedMax = 10206,   // ��������������ٶ�
        kOverloadFrontSpeedAccelerate = 10207,   // ������ǰ�߼��ٶ�
        kOverloadBackSpeedAccelerate = 10208,   // ��������߼��ٶ�
        kOverloadRightSpeedAccelerate = 10209,   // ���������߼��ٶ�
        kOverloadLeftSpeedAccelerate = 10210,   // ���������߼��ٶ�
        kOverloadUpSpeedAccelerate = 10211,   // ���������߼��ٶ�
        kOverloadDownSpeedAccelerate = 10212,   // ���������߼��ٶ�
        kOverloadFrontBackSpeedDecelerate = 10213,   // ����ǰ���������ٶ�
        kOverloadLeftRightSpeedDecelerate = 10214,   // ���������������ٶ�
        kOverloadUpDownSpeedDecelerate = 10215,   // ���������������ٶ�
        kXOverloadAngleSpeedMax = 10216,   // ����X�������ٶ�
        kYOverloadAngleSpeedMax = 10217,   // ����Y�������ٶ�
        kZOverloadAngleSpeedMax = 10218,   // ����Z�������ٶ�
        kXOverloadAngleSpeedAccelerate = 10219,   // ����X��Ǽ��ٶ�
        kYOverloadAngleSpeedAccelerate = 10220,   // ����Y��Ǽ��ٶ�
        kZOverloadAngleSpeedAccelerate = 10221,   // ����Z��Ǽ��ٶ�
        kXOverloadAngleSpeedDecelerate = 10222,   // ����X�������Ǽ��ٶ�
        kYOverloadAngleSpeedDecelerate = 10223,   // ����Y�������Ǽ��ٶ�
        kZOverloadAngleSpeedDecelerate = 10224,   // ����Z�������Ǽ��ٶ�
        kXOverloadMaxAngle = 10225,   // ����X��������
        kYOverloadMaxAngle = 10226,   // ����Y��������
        kZOverloadMaxAngle = 10227,   // ����Z��������
        kXOverloadMimicryAngleSpeedAccelerate = 10228,   // ������̬X�����Ǽ��ٶ�
        kYOverloadMimicryAngleSpeedAccelerate = 10229,   // ������̬Y�����Ǽ��ٶ�
        kZOverloadMimicryAngleSpeedAccelerate = 10230,   // ������̬Z�����Ǽ��ٶ�
        kXOverloadMimicryMaxAngle = 10231,   // ������̬X�����Ƕ�
        kYOverloadMimicryMaxAngle = 10232,   // ������̬Y�����Ƕ�
        kZOverloadMimicryMaxAngle = 10233,   // ������̬Z�����Ƕ�
        kOverloadPowerCostEfficiency = 10299,   // ���ص�������Ч��
        kHPMax = 101,   // װ�����ֵ
        kHP = 102,   // װ�׵�ǰֵ
        kShieldMax = 103,   // �������ֵ
        kShieldValue = 104,   // ���ܵ�ǰֵ
        kPowerMax = 105,   // �������ֵ
        kPowerValue = 106,   // ������ǰֵ
        kCartridgeCapacity = 201,   // �ӵ���������
        kCartridgeReloadTime = 202,   // �ӵ�����װ��ʱ��
        kCartridgeCost = 203,   // �ӵ��������ĵ�ҩ����
        kGuidanceCapacity = 204,   // �Ƶ���������
        kGuidanceReloadTime = 205,   // �Ƶ����ֻظ�ʱ��
        kGuidanceCost = 206,   // �Ƶ����������Ƶ�����
        kLightHeatMax = 207,   // ������������
        kLightHeatCDTime = 208,   // ����������ȴʱ��
        kLightHeatCDEfficiency = 209,   // ����������ȴЧ��
        kLightHeatSafeValue = 210,   // ����������ȫ��ֵ
        kLightHeatIncrementOnce = 211,   // ����������������
        kConverterMax = 212,   // ת��¯��������
        kConverterAccumulateEfficiency = 213,   // ת��¯��������Ч��
        kConverterCostEfficiency = 214,   // ת��¯��������Ч��
        kConverterValue = 215,   // ת��¯������ǰֵ
		kLightHeatCodingTime = 216,   // ������ȴʱ����
		kLightHeatCastSpeed = 217,   // ��������
		kWeaponAccuracy = 250,   // ׼ȷ��
		kWeaponStability = 251,   // �ȶ���
		kRoleLevel = 301,   // ��ɫ�ȼ�
        kWarShipLevel = 302,   // ս���ȼ�
        kDuanLevel = 303,   // ��λ�ȼ�
        kPickupDistance = 320,   // ʰȡ��Χ
        kSpeedWalk = 401,   // �����ٶ�
        kSpeedRun = 402,   // �ܲ��ٶ�
        kSpeedTurn = 403,   // ת���ٶ�
        kSpeedJump = 404,   // �����ٶ�
        kWatchpointY = 405,   // �۲��ƫ��Y
        kWatchpointX = 406,   // �۲��ƫ��X
        kCameraNear = 407,   // �����������
        kCameraFar = 408,   // �����Զ����
        kAttack = 501,   // ����
        kDefend = 502,   // ����
        kInvisible = 601,   // ����
        kAttributeNameMax = 10299
    }
}