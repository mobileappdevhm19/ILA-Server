module.exports = (sequelize, DataTypes) => {
    const Course = sequelize.define('course_tokens', {
        id: {
            type: DataTypes.INTEGER,
            primaryKey: true,
            autoIncrement: true
        },
        token: {
            type: DataTypes.STRING,
            length: 10,
            allowNull: false
        },
        enabled: {
            type: DataTypes.BOOLEAN,
            allowNull: false,
        }
    }, {
        timestamps: false
    });

    Course.associate = models => {
        Course.belongsTo(models.Course, {foreignKey: 'courseId', as: 'course'});
    };

    return Course;
};

