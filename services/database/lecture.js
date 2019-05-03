module.exports = (sequelize, DataTypes) => {
    const Course = sequelize.define('lectures', {
        id: {
            type: DataTypes.INTEGER,
            primaryKey: true,
            autoIncrement: true
        },
        topic: {
            type: DataTypes.STRING,
            allowNull: false
        },
        description: DataTypes.STRING,
        visible: {
            type: DataTypes.BOOLEAN,
            allowNull: false
        }
        // owner
    }, {
        timestamps: false
    });

    Course.associate = models => {
        Course.belongsTo(models.Course, {foreignKey: 'courseId'});
    };

    return Course;
};

