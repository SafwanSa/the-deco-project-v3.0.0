from django.shortcuts import render
from rest_framework.views import APIView
from .serializers import *
from rest_framework.response import Response
from django.db.models import Q
import json
from PIL import ImageColor

class DecoView(APIView):

    def post(self, request):
        data_list = request.data["list"]
        serializer = DecoSerializer(data=data_list, many=True)
        serializer.is_valid(raise_exception=True)
        names = []
        for data in serializer.validated_data:
            dna = data.pop('dna')                    
            deco, created = Deco.objects.get_or_create(**data)
            if len(data['parentsNames']) != 0 and created:
                parents_names = [n.strip() for n in data['parentsNames'].split(',')]
                parents = Deco.objects.filter(name__in=parents_names)
                father_name = deco.name[0:len(deco.name)-1]
                father = parents.filter(name=father_name).first()
                mother = parents.filter(~Q(name=father_name)).first()
                deco.father = father
                deco.mother = mother
                deco.save()
                
            names.append(deco.name)
            dd = DNA(deco=deco, **dna)
            dd.save()
        
        population = Population.objects.create(population=len(names))
        died_decos = Deco.objects.exclude(name__in=names)
        for deco in died_decos:
            deco.is_died = True
            deco.save()

        return Response({"message": "Done"})

    def get(self, request):
        first_men = Deco.objects.filter(generationTag=1)
        serializer = DD(first_men, many=True)
        data = {"Root": {"children": serializer.data}}
        names = []
        for d in Deco.objects.all():
            rgb = ImageColor.getcolor(d.color, "RGB")
            font_color = "black" if (0.299*rgb[0]+0.587*rgb[1]+0.114*rgb[2])/255 > 0.5 else "white"
            st = f'{d.name}_{d.generationTag} [style=filled fontcolor="{font_color}" fillcolor="{d.color}" fixedsize=false]'
            print(st)
            names.append(st)
            
            
        with open('styles.json', 'w') as outfile:
            json.dump(names, outfile)
            
        with open('data.json', 'w') as outfile:
            json.dump(data, outfile)
        return Response(data)