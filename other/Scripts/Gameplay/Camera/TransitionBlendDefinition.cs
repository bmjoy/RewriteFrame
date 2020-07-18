using static Cinemachine.CinemachineVirtualCameraBase;
using static MainCameraTransitionBlenderSettings;

public struct TransitionBlendDefinition
{
	public readonly BlendHint BlendHint;
	public readonly bool InheritPosition;

	public TransitionBlendDefinition(CustomBlend customBlend)
	{
		BlendHint = customBlend.BlendHint;
		InheritPosition = customBlend.InheritPosition;
	}

	public TransitionBlendDefinition(BlendHint blendHint, bool inheritPosition)
	{
		BlendHint = blendHint;
		InheritPosition = inheritPosition;
	}
}