"use strict";

var answersCtx = document.getElementById('answersChart').getContext('2d');
var answersChart = new Chart(answersCtx, {
  type: 'pie',
  options: {
    animation: {
      duration: 0
    }
  }
});

var connection = new signalR.HubConnectionBuilder().withUrl("/lectureHub").build();

function updateChart() {
  connection.invoke("GetAnswers", questionId)
    .then((question) => {
      console.log(question);
      var chartData = {
        datasets: [{
        backgroundColor: null,
          data: []
        }],
        labels: []
      };
      question.answers.forEach((answer) => {
        chartData.labels.push(answer.answer+': '+answer.profQuestionAnswers.length);
        chartData.datasets[0].data.push(answer.profQuestionAnswers.length);
      });

      chartData.datasets[0].backgroundColor = palette('tol', chartData.labels.length).map(function (hex) {
        return '#' + hex;
      });
      answersChart.data = chartData;

      answersChart.update();
    })
    .catch((err) => console.error(err.toString()));
}

async function start() {
  try {
    await connection.start().then(function () {
      console.log("connected");
      connection.invoke("RegisterLecture", lectureId)
        .catch((err) => console.error(err.toString()));
      updateChart();
    });
  } catch (err) {
    console.log(err);
    setTimeout(() => start(), 5000);
  }
};

connection.onclose(async () => {
  await start();
});

connection.on("QuestionChanged", (pause) => {
  updateChart();
});

// Open Connection
start();