using NUnit.Framework;
using UnityEngine;
using BusBoys.Assets.Scripts.Sensors.CollisionReward;
using BusBoys.Assets.Scripts.ML.Rewards;
using System.Reflection;
using BusBoys.Assets.Scripts.Configs;

public class CollisionRewarderTests
{
    // Fake implementation of AgentRewardProvider so we can track rewards  
    private class TestRewardProvider : AgentRewardProvider
    {
        public float? LastRewardAdded = null;

        // Fix: Marking AddReward as new since the base method is not virtual, abstract, or override  
        public new void AddReward(float reward)
        {
            LastRewardAdded = reward;
        }
    }

    private GameObject _go;
    private CollisionRewarder _rewarder;
    private TestRewardProvider _rewardProvider;
    private RewardConfig _rewardConfig;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _rewarder = _go.AddComponent<CollisionRewarder>();
        _rewardProvider = _go.AddComponent<TestRewardProvider>();

        _rewardConfig = ScriptableObject.CreateInstance<RewardConfig>();
        _rewardConfig.collisionPenalty = -10f;
        _rewardProvider.rewardConfig = _rewardConfig;

        SetPrivateField(_rewarder, "rewardProvider", _rewardProvider);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_go);
        Object.DestroyImmediate(_rewardConfig);
    }

    [Test]
    public void HandleCollision_LayerDoesNotMatch_DoesNotAddReward()
    {
        LayerMask mask = LayerMask.GetMask("Default");
        SetPrivateField(_rewarder, "layerMask", mask);

        _rewarder.HandleCollision(LayerMask.NameToLayer("TransparentFX"));

        Assert.IsNull(_rewardProvider.LastRewardAdded);
    }

    [Test]
    public void OnCollisionEnter_NullRewardProvider_DoesNotThrow()
    {
        SetPrivateField(_rewarder, "rewardProvider", null);

        UnityEngine.TestTools.LogAssert.Expect(LogType.Error,
            "Reward provider is not assigned in the inspector, reward will not be calculated.");

        Assert.DoesNotThrow(() => _rewarder.OnCollisionEnter(null));
    }

    [Test]
    public void IsInLayerMask_LayerInMask_ReturnsTrue()
    {
        var layer = LayerMask.NameToLayer("Default");
        var mask = LayerMask.GetMask("Default");
        Assert.IsTrue(CollisionRewarder.IsInLayerMask(layer, mask));
    }

    [Test]
    public void IsInLayerMask_LayerNotInMask_ReturnsFalse()
    {
        var layer = LayerMask.NameToLayer("TransparentFX");
        var mask = LayerMask.GetMask("Default");
        Assert.IsFalse(CollisionRewarder.IsInLayerMask(layer, mask));
    }

    // --- Helpers ---  

    private static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(obj, value);
    }

    private static Collision CreateCollision(GameObject other)
    {
        var collision = (Collision)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(Collision));

        // Find the field that holds a GameObject, regardless of what it's called
        foreach (var field in typeof(Collision).GetFields(
            BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (field.FieldType == typeof(GameObject))
            {
                field.SetValue(collision, other);
                return collision;
            }
        }

        throw new System.Exception("Could not find GameObject field in Collision class");
    }
}
