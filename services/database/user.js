module.exports = (sequelize, DataTypes) => {
    const User = sequelize.define('users', {
        id: {
            type: DataTypes.INTEGER,
            primaryKey: true,
            autoIncrement: true
        },
        name: DataTypes.STRING,
        password: DataTypes.STRING,
        email: DataTypes.STRING,
        emailApproved: DataTypes.BOOLEAN
    }, {
        timestamps: false
    });

    User.associate = models => {
        User.hasMany(models.Course, {onDelete: 'CASCADE', foreignKey: 'ownerId'});
        User.belongsToMany(models.Course, {
            as: 'MemberCourses',
            through: 'course_members',
            foreignKey: 'userId',
            timestamps: false
        });
    };

    return User;
};