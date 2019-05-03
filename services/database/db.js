const Sequelize = require('sequelize');

// load environment variables
require('dotenv').config();

const dbPort = process.env.DB_PORT || 3306;
const dbHost = process.env.DB_HOST || "localhost";
const dbDatabase = process.env.DB_DATABASE || "ila";
const dbUsername = process.env.DB_USERNAME || "root";
const dbPassword = process.env.DB_PASSWORD || "";


const sequelize = new Sequelize(dbDatabase, dbUsername, dbPassword, {
    host: dbHost,
    port: dbPort,
    dialect: 'mariadb',
    pool: {
        min: 0,
        max: 100,
        idle: 10000,
    }
});

const testConnection = () => {
    sequelize
        .authenticate()
        .catch((error) => {
            console.error('Could not connect to the database');
            console.error(JSON.stringify(error));
            process.exit(1);
        });
};

const models = {
    User: sequelize.import('./user'),
    Course: sequelize.import('./course'),
    CourseToken: sequelize.import('./coursetoken'),
    Lecture: sequelize.import('./lecture')
};

Object.keys(models).forEach(key => {
    if ('associate' in models[key]) {
        models[key].associate(models);
    }
});

module.exports = {
    sequelize,
    models,
    testConnection
};