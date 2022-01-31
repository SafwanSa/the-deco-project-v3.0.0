from django.contrib import admin
from .models import *
from django.db.models import Count


class DNAAdminInline(admin.StackedInline):
    model = DNA
    extra = 0

class DecoAdmin(admin.ModelAdmin):
    model = Deco
    inlines = [DNAAdminInline]
    list_display = [
        "name","descendents", 'get_age',"generationTag",'get_first_perception', "get_last_perception", "get_num_of_children"
    ]
    list_filter = [
        'generationTag', "dna__gender", "is_died"
    ]
    
    readonly_fields = ['father', 'mother']
    
    search_fields = ['name']
    
    def get_health(self, obj):
        last_dna = obj.dna_set.last()
        if last_dna:
            return str(last_dna.health)
        return ''
    
    def get_last_perception(self, obj):
        last_dna = obj.dna_set.last()
        if last_dna:
            return str(last_dna.perception)
        return ''
    get_last_perception.short_description = 'Last Perception'
    
    def get_first_perception(self, obj):
        first_dna = obj.dna_set.first()
        if first_dna:
            return str(first_dna.perception)
        return ''
    get_first_perception.short_description = 'First Perception'
    
    def get_num_of_children(self, obj):
        return str(obj.father_children.count())
    get_num_of_children.short_description = 'Num of Children'
    
    def get_queryset(self, request):
        super().get_queryset(request)
        return Deco.objects.annotate(num_children=Count('father_children')).order_by('-num_children')
    
    def descendents(self, obj):
        return str(self.calculate_children(obj))
    descendents.short_description = 'Descenteds'
        
    def calculate_children(self, deco):
        if deco.father_children.exists():
            num = 0
            for deco_child in deco.father_children.all():
                num += self.calculate_children(deco_child)
            return (num + deco.father_children.count())
        else:
            return deco.father_children.count()
    
    def get_age(self, obj):
        last_dna = obj.dna_set.last()
        if last_dna:
            return str(last_dna.reportedAtGeneration - obj.generationTag)
        return '-'
    get_age.short_description = 'Age'

class DNAAdmin(admin.ModelAdmin):
    model = DNA
    list_display = [
        "get_name", "health", "perception", 'size', 'maxSpeed', "gender", "created_at", "createdAt"
    ]
    list_filter = [
        'deco__generationTag', "gender", "deco__is_died", 'reportedAtGeneration', 'deco__family'
    ]
    
    search_fields = ['deco__name']
    
    def get_name(self, obj):
        return obj.deco.name
    
    def get_created_at(self, obj):
        return obj.created_at.strftime('%H:%M:%S.%f')


class PopulationAdmin(admin.ModelAdmin):
    model = Population
    list_display = ['reported_at', 'population']
admin.site.register(Deco, DecoAdmin)
admin.site.register(DNA, DNAAdmin)
admin.site.register(Population, PopulationAdmin)
