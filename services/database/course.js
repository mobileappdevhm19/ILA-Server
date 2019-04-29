module.exports = (sequelize, DataTypes) => {
    const Course = sequelize.define('courses', {
        id: {
            type: DataTypes.INTEGER,
            primaryKey: true,
            autoIncrement: true
        },
        title: {
            type: DataTypes.STRING,
            allowNull: false
        },
        description: DataTypes.STRING,
        // owner
    }, {
        timestamps: false
    });

    Course.associate = models => {
        Course.belongsTo(models.User, {foreignKey: 'ownerId'});
    };

    return Course;
};

