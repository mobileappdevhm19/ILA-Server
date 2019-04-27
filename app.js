// import all libaries
const express = require('express');
const bodyparser = require('body-parser');
const passport = require('passport');
const database = require('./services/database/db');


// load environment variables
require('dotenv').config();

const port = process.env.PORT || 3000;

// create express server
const app = express();

// register express middlewares
app.use(passport.initialize());
app.use(bodyparser.urlencoded({extended: false}));
app.use(bodyparser.json());

require('./services/passport/passport-config')(passport);

app.use((req, res, next) => {
    console.info(`Received a ${req.method} request from ${req.ip} for ${req.url}`);
    next();
});

// register routes
app.use('/auth', require('./routes/auth'));

// Start server
app.listen(port, err => {
    if (err) console.error(err);
    console.log(`ILA-Server listening on port: ${port}`);
});
