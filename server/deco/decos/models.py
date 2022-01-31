from django.db import models




class Deco(models.Model):
    name = models.TextField(unique=True)
    parentsNames = models.TextField()
    generationTag = models.IntegerField()
    is_died = models.BooleanField(default=False)
    father = models.ForeignKey(
        'self', blank=True, null=True, on_delete=models.CASCADE, related_name='father_children')
    mother = models.ForeignKey(
        'self', blank=True, null=True, on_delete=models.CASCADE, related_name='mother_children')
    color = models.CharField(max_length=7)
    family = models.CharField(max_length=1)
    
    def __str__(self) -> str:
        return self.name
    
class DNA(models.Model):
    created_at = models.TimeField(auto_now=True)
    health = models.FloatField()
    size = models.FloatField()
    gender = models.IntegerField()
    perception = models.FloatField()
    maxSpeed = models.FloatField()
    reportedAtGeneration = models.IntegerField()
    deco = models.ForeignKey(Deco, on_delete=models.CASCADE)
    createdAt = models.FloatField()
    
    class Meta:
        ordering = ('createdAt',)
        
class Population(models.Model):
    reported_at = models.DateTimeField(auto_now=True)
    population = models.IntegerField()