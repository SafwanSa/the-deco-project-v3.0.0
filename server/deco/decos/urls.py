from django.urls import path
from .views import *

urlpatterns = [
    path('', DecoView.as_view(), name='decos')
]
