theApp.controller('chartJsController',
['$http', '$scope', '$rootScope', 'uidlService', '$timeout',
function ($http, $scope, $rootScope, uidlService, $timeout) {

    $scope.drawChart = function() {
        $timeout(function() {
            var ctx = document.getElementById($scope.uidl.qualifiedName + "_context").getContext("2d");
            $scope.chartJsObject = new Chart(ctx).Line($scope.lineData, $scope.lineOptions);
        });
    };

    $scope.$on("$destroy", function() {
    });            

	$scope.$watch('model.State.data', function (newValue, oldValue) {
        if ($scope.chartJsObject) {
            $scope.chartJsObject.destroy();
            $scope.chartJsObject = null;
        }

        if (!newValue) {
            return;
        }
        
		var chartStyles = [
			{
				fillColor: "rgba(220,220,220,0.5)",
				strokeColor: "rgba(220,220,220,1)",
				pointColor: "rgba(220,220,220,1)",
				pointStrokeColor: "#fff",
				pointHighlightFill: "#fff",
				pointHighlightStroke: "rgba(220,220,220,1)"
			},
			{
				fillColor: "rgba(26,179,148,0.5)",
				strokeColor: "rgba(26,179,148,0.7)",
				pointColor: "rgba(26,179,148,1)",
				pointStrokeColor: "#fff",
				pointHighlightFill: "#fff",
				pointHighlightStroke: "rgba(26,179,148,1)"
			}
		];
		
		var chartData = {
			labels: newValue.Labels,
			datasets: [],
			summaries: []
		};
		
		for (var i=0; i<newValue.Series.length; i++){
			chartData.datasets.push(chartStyles[i]);
			chartData.datasets[i].label = newValue.Series[i].Label;
			chartData.datasets[i].data = newValue.Series[i].Values;
		}

        if (newValue.Summaries) {
            for (var i=0; i<newValue.Summaries.length; i++){
                chartData.summaries.push(newValue.Summaries[i]);
            }
        }

		$scope.lineData = chartData;
			
		$scope.lineOptions = {
			scaleShowGridLines: true,
			scaleGridLineColor: "rgba(0,0,0,.05)",
			scaleGridLineWidth: 1,
			bezierCurve: true,
			bezierCurveTension: 0.4,
			pointDot: true,
			pointDotRadius: 4,
			pointDotStrokeWidth: 1,
			pointHitDetectionRadius: 20,
			datasetStroke: true,
			datasetStrokeWidth: 2,
			datasetFill: true,
			responsive: true,
		};
			
        $scope.drawChart();
	});
}]);
