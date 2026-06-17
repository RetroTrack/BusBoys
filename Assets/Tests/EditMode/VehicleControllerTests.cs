using NUnit.Framework;
using UnityEngine;
using BusBoys.Assets.Scripts.Vehicles.Common;
using System.Collections.Generic;
using System.Reflection;

public class VehicleControllerTests
{
    // Concrete implementation since VehicleController is abstract
    private class TestVehicleController : VehicleController { }

    private GameObject _go;
    private TestVehicleController _controller;
    private Rigidbody _rb;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _rb = _go.AddComponent<Rigidbody>();
        _controller = _go.AddComponent<TestVehicleController>();
        SetField(_controller, "rb", _rb);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_go);
    }

    // ── SetInputs ──────────────────────────────────────────────────────

    [Test]
    public void SetInputs_MotorInputAboveOne_ClampsToOne()
    {
        _controller.SetInputs(5f, 0f, 0f);
        Assert.AreEqual(1f, GetField<float>(_controller, "motorInput"));
    }

    [Test]
    public void SetInputs_MotorInputBelowNegativeOne_ClampsToNegativeOne()
    {
        _controller.SetInputs(-5f, 0f, 0f);
        Assert.AreEqual(-1f, GetField<float>(_controller, "motorInput"));
    }

    [Test]
    public void SetInputs_BrakeInputNegative_ClampsToZero()
    {
        _controller.SetInputs(0f, -1f, 0f);
        Assert.AreEqual(0f, GetField<float>(_controller, "brakeInput"));
    }

    [Test]
    public void SetInputs_BrakeInputAboveOne_ClampsToOne()
    {
        _controller.SetInputs(0f, 5f, 0f);
        Assert.AreEqual(1f, GetField<float>(_controller, "brakeInput"));
    }

    [Test]
    public void SetInputs_SteeringAboveOne_ClampsToOne()
    {
        _controller.SetInputs(0f, 0f, 5f);
        Assert.AreEqual(1f, GetField<float>(_controller, "steeringInput"));
    }

    // ── CurrentSpeedNormalized ─────────────────────────────────────────

    [Test]
    public void CurrentSpeedNormalized_HalfMaxSpeed_ReturnsPointFive()
    {
        SetField(_controller, "currentSpeed", 22.5f);
        _controller.maxSpeed = 45f;

        Assert.AreEqual(0.5f, _controller.CurrentSpeedNormalized);
    }

    // ── ResetVehicle ───────────────────────────────────────────────────

    [Test]
    public void ResetVehicle_WhenRigidbodyIsNull_LogsError()
    {
        SetField(_controller, "rb", null);
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error,
            "Rigidbody reference is missing. Cannot reset vehicle.");

        _controller.ResetVehicle();
    }

    [Test]
    public void ResetVehicle_ResetsAllInputsAndSpeedToZero()
    {
        _controller.SetInputs(1f, 1f, 1f);
        SetField(_controller, "currentSpeed", 30f);
        SetField(_controller, "currentSteerAngle", 15f);

        _controller.ResetVehicle();

        Assert.AreEqual(0f, GetField<float>(_controller, "motorInput"));
        Assert.AreEqual(0f, GetField<float>(_controller, "brakeInput"));
        Assert.AreEqual(0f, GetField<float>(_controller, "steeringInput"));
        Assert.AreEqual(0f, _controller.CurrentSpeed);
        Assert.AreEqual(0f, _controller.CurrentSteerAngle);
    }

    // ── Accelerate ─────────────────────────────────────────────────────

    [Test]
    public void Accelerate_WhenSpeedExceedsMax_DoesNotApplyTorque()
    {
        var frontWheel = CreateWheelChild();
        SetField(_controller, "frontWheelColliders", new List<WheelCollider> { frontWheel });

        _controller.driveType = DriveType.FrontWheelDrive;
        _controller.SetInputs(1f, 0f, 0f);
        SetField(_controller, "currentSpeed", 999f);

        _controller.Accelerate();

        Assert.AreEqual(0f, frontWheel.motorTorque);
    }

    [Test]
    public void Accelerate_FrontWheelDrive_AppliesOnlyToFrontWheels()
    {
        var frontWheel = CreateWheelChild();
        var rearWheel = CreateWheelChild();
        SetField(_controller, "frontWheelColliders", new List<WheelCollider> { frontWheel });
        SetField(_controller, "rearWheelColliders", new List<WheelCollider> { rearWheel });

        _controller.driveType = DriveType.FrontWheelDrive;
        _controller.SetInputs(1f, 0f, 0f);
        SetField(_controller, "currentSpeed", 0f);

        _controller.Accelerate();

        Assert.Greater(frontWheel.motorTorque, 0f);
        Assert.AreEqual(0f, rearWheel.motorTorque);
    }

    [Test]
    public void Accelerate_RearWheelDrive_AppliesOnlyToRearWheels()
    {
        var frontWheel = CreateWheelChild();
        var rearWheel = CreateWheelChild();
        SetField(_controller, "frontWheelColliders", new List<WheelCollider> { frontWheel });
        SetField(_controller, "rearWheelColliders", new List<WheelCollider> { rearWheel });

        _controller.driveType = DriveType.RearWheelDrive;
        _controller.SetInputs(1f, 0f, 0f);
        SetField(_controller, "currentSpeed", 0f);

        _controller.Accelerate();

        Assert.AreEqual(0f, frontWheel.motorTorque);
        Assert.Greater(rearWheel.motorTorque, 0f);
    }

    // ── Brake ──────────────────────────────────────────────────────────

    [Test]
    public void Brake_FrontWheelBraking_AppliesOnlyToFrontWheels()
    {
        var frontWheel = CreateWheelChild();
        var rearWheel = CreateWheelChild();
        SetField(_controller, "frontWheelColliders", new List<WheelCollider> { frontWheel });
        SetField(_controller, "rearWheelColliders", new List<WheelCollider> { rearWheel });

        _controller.brakingType = BrakingType.FrontWheelBraking;
        _controller.SetInputs(0f, 1f, 0f);

        _controller.Brake();

        Assert.Greater(frontWheel.brakeTorque, 0f);
        Assert.AreEqual(0f, rearWheel.brakeTorque);
    }

    [Test]
    public void Brake_RearWheelBraking_AppliesOnlyToRearWheels()
    {
        var frontWheel = CreateWheelChild();
        var rearWheel = CreateWheelChild();
        SetField(_controller, "frontWheelColliders", new List<WheelCollider> { frontWheel });
        SetField(_controller, "rearWheelColliders", new List<WheelCollider> { rearWheel });

        _controller.brakingType = BrakingType.RearWheelBraking;
        _controller.SetInputs(0f, 1f, 0f);

        _controller.Brake();

        Assert.AreEqual(0f, frontWheel.brakeTorque);
        Assert.Greater(rearWheel.brakeTorque, 0f);
    }

    // ── GetWheelEncoderValues ──────────────────────────────────────────

    [Test]
    public void GetWheelEncoderValues_FrontWheels_ReturnsCorrectCount()
    {
        SetField(_controller, "frontWheelColliders",
            new List<WheelCollider> { CreateWheelChild(), CreateWheelChild() });

        var result = _controller.GetWheelEncoderValues(WheelType.Front);

        Assert.AreEqual(2, result.Length);
    }

    [Test]
    public void GetWheelEncoderValues_BothWheelTypes_ReturnsCombinedCount()
    {
        SetField(_controller, "frontWheelColliders", new List<WheelCollider> { CreateWheelChild() });
        SetField(_controller, "rearWheelColliders", new List<WheelCollider> { CreateWheelChild() });

        var result = _controller.GetWheelEncoderValues(WheelType.Both);

        Assert.AreEqual(2, result.Length);
    }

    // ── Helpers ────────────────────────────────────────────────────────

    private WheelCollider CreateWheelChild()
    {
        var child = new GameObject();
        child.transform.SetParent(_go.transform);
        return child.AddComponent<WheelCollider>();
    }

    private static void SetField(object obj, string name, object value)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var field = type.GetField(name,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (field != null) { field.SetValue(obj, value); return; }
            type = type.BaseType;
        }
        throw new System.Exception($"Field '{name}' not found");
    }

    private static T GetField<T>(object obj, string name)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var field = type.GetField(name,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (field != null) return (T)field.GetValue(obj);
            type = type.BaseType;
        }
        throw new System.Exception($"Field '{name}' not found");
    }
}