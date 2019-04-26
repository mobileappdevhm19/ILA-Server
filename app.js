// import all libaries
const express = require('express');
const bodyparser = require('body-parser');
const passport = require('passport');

// load environment variables
require('dotenv').config();

const port = process.env.PORT || 3000;
const dbPort = process.env.DB_PORT || 3306;
const dbHost = process.env.DB_HOST || "localhost";
const dbDatabase = process.env.DB_DATABASE || "ila";
const dbUsername = process.env.DB_USERNAME || "root";
const dbPassword = process.env.DB_PASSWORD || "";

// TODO: check database connection

// create express server
const app = express();

// register express middlewares
app.use(passport.initialize());
app.use(bodyparser.urlencoded({extended: false}));
app.use(bodyparser.json());

// TODO: initialize passport
//require('./security/passport-config')(passport);


// TODO: logging only in development mode
app.use((req, res, next) => {
    console.info(`Received a ${req.method} request from ${req.ip} for ${req.url}`);
    next();
});

// register routes
//app.use('/users', require('./routes/user'))

// Start server
app.listen(port, err => {
    if (err) console.error(err);
    console.log(`ILA-Server listening on port: ${port}`);
});