using UnityEngine;

public static class hwmMath
{
    /// <summary>
    /// Gets the reciprocal of this vector, avoiding division by zero.
    /// Zero components are set to float.MaxValue.
    /// </summary>
    /// <param name="vec"></param>
    /// <returns>Reciprocal of this vector.</returns>
    public static Vector2 Reciprocal(Vector2 vec)
    {
        return new Vector2(vec.x != 0.0f ? 1.0f / vec.x : float.MaxValue
            , vec.y != 0.0f ? 1.0f / vec.y : float.MaxValue);
    }

    /// <summary>
    /// Transforms a direction by this matrix.
    /// </summary>
    public static Vector2 MatrixMultiplyVector(Matrix4x4 matrix, Vector2 vector)
    {
        Vector2 vector2;
        vector2.x = matrix.m00 * vector.x + matrix.m01 * vector.y;
        vector2.y = matrix.m10 * vector.x + matrix.m11 * vector.y;
        return vector2;
    }

    public static Vector2 QuaternionMultiplyVector(Quaternion rotation, Vector2 vector)
    {
        float num1 = rotation.x * 2f;
        float num2 = rotation.y * 2f;
        float num3 = rotation.z * 2f;
        float num4 = rotation.x * num1;
        float num5 = rotation.y * num2;
        float num6 = rotation.z * num3;
        float num7 = rotation.x * num2;
        float num12 = rotation.w * num3;
        Vector2 vector2;
        vector2.x = (1.0f - (num5 + num6)) * vector.x + (num7 - num12) * vector.y;
        vector2.y = (num7 + num12) * vector.x + (1.0f - (num4 + num6)) * vector.y;
        return vector2;
    }

    public static float Square(float value)
    {
        return value * value;
    }

    public static float Max(float a, float b, float c, float d)
    {
        return Max(Max(a, b), Max(c, d));
    }

    public static float Max(float a, float b)
    {
        return a > b ? a : b;
    }

    public static float Min(float a, float b, float c, float d)
    {
        return Min(Min(a, b), Min(c, d));
    }

    public static float Min(float a, float b)
    {
        return a < b ? a : b;
    }

    public static float ClampAbs(float value, float maxAbs)
    {
        return Mathf.Sign(value) * Mathf.Min(Mathf.Abs(value), maxAbs);
    }

    /// <summary>
    /// 计算施加推力后，产生的加速度
    /// kinetic energy：E_k = 1 / 2 * m * v^2 <see cref="https://en.wikipedia.org/wiki/Kinetic_energy"/>
    /// </summary>
    /// <param name="power">推力</param>
    /// <param name="currentSpeed">当前速度</param>
    /// <param name="mass">质量</param>
    /// <param name="delta">持续时间</param>
    /// <returns>加速度</returns>
    public static float PowerToAcceleration(float power, float currentSpeed, float mass, float delta)
    {
        Debug.Assert(mass > Mathf.Epsilon, "mass > Mathf.Epsilon");

        // 施加推力后的动能
        float kineticEnergy = Mathf.Sign(currentSpeed) * 0.5f * mass * currentSpeed * currentSpeed // 当前的动能
            + power * delta;

        //kineticEnergy = Mathf.Max(0, kineticEnergy); // HACK 减速时，如果速度过慢，动能可能小于0。暂不支持飞机倒飞，所以动能最小为0
        float newSpeed = Mathf.Sign(kineticEnergy) 
            * Mathf.Sqrt(
                Mathf.Abs(kineticEnergy) * 2.0f / mass); // 用动能算速度
        return (newSpeed - currentSpeed) / delta;
    }

    /// <summary>
    /// 计算物体受到的空气阻力
    /// 
    /// 参考流体力学的阻力方程：F_D = 0.5 * p * v^2 * C_D * A <see cref="https://en.wikipedia.org/wiki/Drag_equation"/>
    /// F_D: 阻力
    /// ρ: 流体密度
    /// v: 物体速度
    /// A: 参考面积
    /// C_D: 阻力系数, 是一个无因次的系数, 像汽车的阻力系数约在0.25到0.4之间
    /// 
    /// 计算的阻力是有方向的, 且方向和速度相反
    /// 降低复杂度, 忽略流体密度和参考面积
    /// 所以这里用公式
    ///     F_D = 0.5 * (-v * |v|) * C_D
    /// </summary>
    /// <param name="velocity">速度(v)</param>
    /// <param name="dragCoefficient">阻力系数(C_D)</param>
    /// <returns>速度相反方向的阻力(F_D)</returns>
    public static Vector3 CalculateDrag(Vector3 velocity, Vector3 dragCoefficient)
    {
        // -v * |v|
        Vector3 dragForce = new Vector3(-velocity.x * Mathf.Abs(velocity.x),
            -velocity.y * Mathf.Abs(velocity.y),
            -velocity.z * Mathf.Abs(velocity.z));

        // (-v * |v|) * C_D
        dragForce.Scale(dragCoefficient);

        // 0.5 * (-v * |v|) * C_D
        return dragForce * 0.5f;
    }

    /// <summary>
    /// 匀变速运动
    /// <see cref="https://en.wikipedia.org/wiki/Acceleration#Uniform_acceleration"/>
    /// </summary>
    public static float UniformAccelerationTo(float current
        , float target
        , ref float velocity
        , float maxVelocity
        , float maxAcceleration
        , float deltaTime
        , float invertDeltaTime)
    {
        float currentToTargetDistance = target - current;
        float currentToTargetDirection = Mathf.Sign(currentToTargetDistance);
        currentToTargetDistance = currentToTargetDistance * currentToTargetDirection; // 相当于取绝对值

        // Vt^2 - V0^2 = 2*a*s
        float fCurrentIdealSpeed = Mathf.Sqrt(2.0f * maxAcceleration * currentToTargetDistance);
        float fNewIdealSpeed = Mathf.Max(0, fCurrentIdealSpeed - maxAcceleration * deltaTime);

        float vNewVelocity = Mathf.MoveTowards(velocity, currentToTargetDirection * fNewIdealSpeed, maxAcceleration * deltaTime);
        vNewVelocity = ClampAbs(vNewVelocity, maxVelocity);

        float a = (vNewVelocity - velocity) * invertDeltaTime;
        float vDelta = velocity * deltaTime + 0.5f * a * deltaTime * deltaTime;
        current += vDelta;
        velocity = vNewVelocity;

        return current;
    }

    /// <summary>
    /// 匀变速运动
    /// <see cref="UniformAccelerationTo(float, float, ref float, float, float, float, float)"/>
    /// </summary>
    public static Vector3 UniformAccelerationTo(Vector3 current
        , Vector3 target
        , ref Vector3 velocity
        , Vector3 maxVelocity
        , Vector3 maxAcceleration
        , float deltaTime
        , float invertDeltaTime)
    {
        return new Vector3(UniformAccelerationTo(current.x, target.x, ref velocity.x, maxVelocity.x, maxAcceleration.x, deltaTime, invertDeltaTime)
            , UniformAccelerationTo(current.y, target.y, ref velocity.y, maxVelocity.y, maxAcceleration.y, deltaTime, invertDeltaTime)
            , UniformAccelerationTo(current.z, target.z, ref velocity.z, maxVelocity.z, maxAcceleration.z, deltaTime, invertDeltaTime));
    }

    /// <summary>
    /// 把角度(Glossary?)到 -180~180
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static float NormalizeAngle180(float angle)
    {
        angle = angle % 360;
        return angle >= 180 ? angle - 360 : angle;
    }

    /// <summary>
    /// 把角度(Glossary?)到 -180~180
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static Vector3 NormalizeAngle180(Vector3 angle)
    {
        return new Vector3(NormalizeAngle180(angle.x)
            , NormalizeAngle180(angle.y)
            , NormalizeAngle180(angle.z));
    }
}