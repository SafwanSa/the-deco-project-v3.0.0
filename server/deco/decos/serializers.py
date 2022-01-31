from rest_framework import serializers
from .models import *


class DNASerializer(serializers.Serializer):
    health = serializers.FloatField()
    size = serializers.FloatField()
    gender = serializers.IntegerField()
    perception = serializers.FloatField()
    size = serializers.FloatField()
    maxSpeed = serializers.FloatField()
    reportedAtGeneration = serializers.IntegerField()
    createdAt = serializers.FloatField()

    


class DecoSerializer(serializers.Serializer):
    dna = DNASerializer()
    name = serializers.CharField()
    parentsNames = serializers.CharField()
    generationTag = serializers.IntegerField()
    color = serializers.CharField()
    family = serializers.CharField()
    
    def to_internal_value(self, data):
        parentsNames = data["parentsNames"]
        data["parentsNames"] = '-'
        
        if len(parentsNames) != 0:
            data["parentsNames"] =', '.join(parentsNames)


        
        return super().to_internal_value(data)


class RecursiveField(serializers.ModelSerializer): 
    def to_representation(self, value):
        serializer_data = DD(value, context=self.context).data
        return serializer_data
    class Meta:
            model = Deco
            fields = ('name', 'father_children', 'generationTag')
            
class DD(serializers.ModelSerializer):
    father_children = RecursiveField(allow_null=True, many=True)

    class Meta:
        model = Deco
        fields = ('name', 'father_children', 'generationTag')
        
    def to_representation(self, instance):
        representation =  super().to_representation(instance)
        name =  representation.pop('name') + "_"+str(representation.pop('generationTag'))
        representation[name] = dict()
        children = representation.pop('father_children')
        representation[name]["children"] = children
        return representation