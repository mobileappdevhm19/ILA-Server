"use strict";

var color = Chart.helpers.color;
// Chart
var pauseCtx = document.getElementById('pauseChart').getContext('2d');
var pauseChart = new Chart(pauseCtx, {
  type: 'line',
  data: {
    datasets: [
      {
        label: 'Pauses: ',
        backgroundColor: color('rgb(255, 99, 132)').alpha(0.5).rgbString(),
        borderColor: 'rgb(255, 99, 132)',
        data: [],
        type: 'line',
        pointRadius: 2,
        pointHoverRadius: 10,
        fill: false,
        lineTension: 0,
        borderWidth: 2,
        showLine: false
      }
    ]
  },
  options: {
    scales: {
      xAxes: [{
        type: 'time',
        time: {
          unit: 'minute',
          distribution: 'series',
          min: lectureStart,
          max: lectureEnd
        }
      }],
      yAxes: [{
        scaleLabel: {
          display: true,
          labelString: 'Pauses'
        }
      }]
    }
  }
});

var connection = new signalR.HubConnectionBuilder().withUrl("/lectureHub").build();
var pauses = [];

async function start() {
  try {
    await connection.start().then(function () {
      console.log("connected");
      connection.invoke("RegisterLecture", lectureId)
        .catch((err) => console.error(err.toString()));
      connection.invoke("GetPauses", lectureId)
        .then((data) => {
          data.forEach((pause) => {
            var time = moment.utc(pause.timeStamp).seconds(0).milliseconds(0);
            var flag = false;
            pauseChart.data.datasets[0].data.forEach(pause1 => {
              if (moment.duration(time.diff(pause1.t)).asSeconds() < 1 && !flag) {
                flag = true;
                pause1.y += 1;
              }
            }
            );
            if (!flag)
              pauseChart.data.datasets[0].data.push({
                t: time,
                y: 1
              });
          });
          pauseChart.update();
        })
        .catch((err) => console.error(err.toString()));
    });
  } catch (err) {
    console.log(err);
    setTimeout(() => start(), 5000);
  }
};

connection.onclose(async () => {
  await start();
});

connection.on("Pause", (pause) => {
  var time = moment(pause.timeStamp).seconds(0).milliseconds(0);
  var flag = false;
  pauseChart.data.datasets[0].data.forEach(pause1 => {
    if (moment.duration(time.diff(pause1.t)).asSeconds() < 1 && !flag) {
      flag = true;
      pause1.y += 1;
    }
  }
  );
  if (!flag)
    pauseChart.data.datasets[0].data.push({
      t: time,
      y: 1
    });
  pauseChart.update();
});


// Open Connection
start();