# Clustering
-------------

Во всех алгоритмах использовалась Евклидова метрика.

В K-means и Hierarchical Clustering использовалась min-max нормализация.

В названиях картинок в скобках указывается дополнительная информация о параметрах запуска программы. Запрашиваемый программой параметр, указан в скобках без пояснений, остальные числа пояснением сопровождаются.

K-means (K-means++):

    Входные данные: количество кластеров.
    Начальные средние задаются с помощью метода колеса рулетки.

Hierarchical Clustering:

    Входные данные: количество кластеров.
    Использовался восходящий алгоритм. Дендограмма не хранится. Расстояние между кластерами центроидное невзвешенное.

DBSCAN:

    Входные данные: максимальное расстояние между соседними элементами одного кластера.
    Также, в коде программы можно изменить минимальное количество соседей внутренних точек кластера.
