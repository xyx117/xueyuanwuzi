using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using wzgl.Models;

namespace wzgl.DAL
{
    public class WzglContent: DbContext
    {
        public WzglContent() : base("wzglContent")
        {
            this.Configuration.ProxyCreationEnabled = false;     //关闭关联的子表
        }

        public DbSet<Dingdanzhubiao> Dingdanzhubiaos { get; set; }
        
        public DbSet<Liuchengzhubiao> Liuchengzhubiaos { get; set; }

        public DbSet<Liuchengzibiao> Liuchengzibiaos { get; set; }

        //public DbSet<Shenhezhubiao> Shenhezhubiaos { get; set; }

        public DbSet<Shenhezibiao> Shenhezibiaos { get; set; }

        public DbSet<Xiangmubiao> Xiangmubiaos { get; set; }

        public DbSet<Bumenshezhi> Bumenshezhis { get; set; }

        public DbSet<Mulushezhi> Mulushezhis { get; set; }

        public DbSet<Fapiao_upload> Fapiao_uploads { get; set; }        

        public DbSet<Xiangmuzibiao> Xiangmuzibiaos { get; set; }

        public DbSet<Rukubiao> Rukubiaos { get; set; }

        public DbSet<File_upload> File_uploads { get; set; }

        public DbSet<Fapiao_liucheng_zhubiao> Fapiao_liuchneg_zhubiaos { get; set; }        

        public DbSet<Fapiao_liuchneg_zibiao> Fapiao_liuchneg_zibiaos { get; set; }

        public DbSet<Shenhezibiao_Fapiao> Shenhezibiao_Fapiaos { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            //一对一的 关系需要启动级联删除

            ////项目基本信息表WillCascadeOnDelete(false)
            //modelBuilder.Entity<xmjibenxinxi>().HasRequired(p => p.xiangmuguanli).WithOptional(p => p.xmjibenxinxi).WillCascadeOnDelete(true);//.WillCascadeOnDelete(true)启动级联删除
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            //modelBuilder.Entity<Shenhezhubiao>().HasRequired(p => p.Xiangmubiao).WithOptional();

            //审核主表和项目表之间的一对一关系
            //modelBuilder.Entity<Shenhezhubiao>().HasRequired(p => p.Xiangmubiao).WithOptional(p => p.Shenhezhubiao).WillCascadeOnDelete(true);//.WillCascadeOnDelete(true)启动级联删除

            //modelBuilder.Entity<Dingdanzibiao>().HasRequired(p => p.Dingdanzhubiao).WithOptional(p => p.Dingdanzibiaos).WillCascadeOnDelete(true);//.WillCascadeOnDelete(true)启动级联删除


            //项目表和 订单主表的 级联删除关闭
            //modelBuilder.Entity<Xiangmubiao>().HasOptional<Dingdanzhubiao>(s => s.Dingdanzhubiao).WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<Xiangmubiao>().HasOptional(s => s.Dingdanzhubiao).WithMany().WillCascadeOnDelete(false);

            //级联删除关闭
            //modelBuilder.Entity<Rukubiao>().HasOptional(s => s.Xiangmuzibiao).WithMany().WillCascadeOnDelete(false);
        }
    }
}