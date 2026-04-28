//using UnityEditor;
//using UnityEditor.UIElements;
//using UnityEngine;
//using UnityEngine.UIElements;
//using static EventData;
//using static EventData.EventEffect;

//[CustomPropertyDrawer(typeof(EventEffect))]
//public class EventEffectDrawer : PropertyDrawer {
//    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
//        // Create property container element.
//        var container = new VisualElement();

//        var effectTypeProp = property.FindPropertyRelative("effectType");
//        var effectValueProp = property.FindPropertyRelative("effectValue");

//        // Create property fields.
//        var effectTypeField = new PropertyField(effectTypeProp);
//        var enemyField = new PropertyField(property.FindPropertyRelative("enemy"));
//        var effectValueField = new PropertyField(effectValueProp, "Lose amount");

//        container.Add(effectTypeField);
//        container.Add(effectValueField);
//        container.Add(enemyField);

//        void UpdateVisibility() {
//            var selected = (EffectType)effectTypeProp.enumValueIndex;
//            enemyField.style.display = selected == EffectType.Fight ? DisplayStyle.Flex : DisplayStyle.None;
//            effectValueField.style.display = (selected == EffectType.LoseParts || selected == EffectType.LoseHP) ? DisplayStyle.Flex : DisplayStyle.None;
//        }

//        UpdateVisibility();
//        effectTypeField.RegisterValueChangeCallback(evt => {
//            UpdateVisibility();
//        });

//        return container;
//    }
//}

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static EventEffect;

// Not gonna take credit - this was mostly wibe coded.
[CustomPropertyDrawer(typeof(EventEffect))]
public class EventEffectDrawer : PropertyDrawer {
    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        var root = new VisualElement();

        var typeProp = property.FindPropertyRelative("effectType");
        var effectProp = property.FindPropertyRelative("effectData");

        // Enum field
        var typeField = new PropertyField(typeProp);
        root.Add(typeField);

        // Container for the effect fields
        var container = new VisualElement();
        root.Add(container);

        // Map enum -> type
        Type GetTypeFromEnum(EffectType type) {
            return type switch {
                EffectType.None => typeof(NoneEffect),
                EffectType.Fight => typeof(FightEffect),
                EffectType.Run => typeof(RunEffect),
                EffectType.LoseParts => typeof(LosePartsEffect),
                _ => null,
            };
        }

        void UpdateEffect() {
            var selected = (EffectType)typeProp.enumValueIndex;
            var targetType = GetTypeFromEnum(selected);

            if (targetType == null) {
                effectProp.managedReferenceValue = null;
            }
            else {
                if (effectProp.managedReferenceValue == null ||
                    effectProp.managedReferenceValue.GetType() != targetType) {
                    effectProp.managedReferenceValue = Activator.CreateInstance(targetType);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            // Redraw UI
            container.Clear();

            if (effectProp.managedReferenceValue != null) {
                var field = new PropertyField(effectProp);
                DrawInline(effectProp, container);
                //field.Bind(property.serializedObject);
                //effectProp.isExpanded = true;

                //container.Add(field);
            }
        }

        // Initial draw
        UpdateEffect();

        // React to enum change
        typeField.RegisterValueChangeCallback(evt => {
            UpdateEffect();
        });

        return root;
    }

    void DrawInline(SerializedProperty prop, VisualElement container) {
        prop.isExpanded = true;

        var iterator = prop.Copy();
        var end = iterator.GetEndProperty();

        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end)) {
            enterChildren = false;

            // Skip the internal managed reference metadata
            if (iterator.propertyPath.EndsWith("managedReferenceFullTypename"))
                continue;

            var field = new PropertyField(iterator.Copy());
            field.Bind(prop.serializedObject);
            container.Add(field);
        }
    }
}